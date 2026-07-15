using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using qisu_server.Data;
using qisu_server.Models;

namespace qisu_server.Controllers;

/// <summary>
/// SSE（Server-Sent Events）控制器
/// 用于实现服务器主动推送消息给客户端
/// </summary>
[ApiController]
[Route("api/sse")]
public class SseController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SseController> _logger;
    private static readonly Dictionary<long, List<HttpResponse>> _connections = new();
    private static readonly object _lock = new();
    private static ILogger _staticLogger;

    public SseController(IServiceProvider serviceProvider, ILogger<SseController> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _staticLogger ??= logger;
    }

    /// <summary>
    /// 建立SSE连接
    /// </summary>
    /// <param name="token">JWT令牌，用于验证用户身份</param>
    /// <remarks>
    /// 客户端通过此接口建立长连接，服务器可以主动推送消息
    /// 连接会每30秒发送一次心跳包保持活跃
    /// </remarks>
    [HttpGet("connect")]
    public async Task Connect([FromQuery] string? token)
    {
        long? userId = null;
        
        if (!string.IsNullOrEmpty(token))
        {
            userId = ValidateToken(token);
        }
        
        if (userId == null)
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && long.TryParse(userIdClaim, out var id))
            {
                userId = id;
            }
        }

        if (userId == null)
        {
            Response.StatusCode = 401;
            return;
        }

        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        lock (_lock)
        {
            if (!_connections.ContainsKey(userId.Value))
            {
                _connections[userId.Value] = new List<HttpResponse>();
            }
            _connections[userId.Value].Add(Response);
        }

        _logger.LogInformation("SSE 连接建立: UserId={UserId}", userId);

        try
        {
            await SendEventAsync(Response, "connected", new { message = "SSE 连接成功" });

            var heartbeatInterval = new PeriodicTimer(TimeSpan.FromSeconds(30));
            while (await heartbeatInterval.WaitForNextTickAsync(HttpContext.RequestAborted))
            {
                await SendEventAsync(Response, "heartbeat", new { time = DateTime.Now });
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("SSE 连接断开: UserId={UserId}", userId);
        }
        finally
        {
            lock (_lock)
            {
                if (_connections.ContainsKey(userId.Value))
                {
                    _connections[userId.Value].Remove(Response);
                    if (_connections[userId.Value].Count == 0)
                    {
                        _connections.Remove(userId.Value);
                    }
                }
            }
        }
    }

    private long? ValidateToken(string token)
    {
        try
        {
            _logger.LogInformation("开始验证 SSE Token");
            var jwtService = _serviceProvider.GetService<Services.IJwtService>();
            if (jwtService == null)
            {
                _logger.LogWarning("JwtService 未注册");
                return null;
            }

            var principal = jwtService.ValidateToken(token);
            if (principal == null)
            {
                _logger.LogWarning("Token 验证失败");
                return null;
            }

            var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                userIdClaim = principal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            }
            if (string.IsNullOrEmpty(userIdClaim))
            {
                userIdClaim = principal.FindFirst("sub")?.Value;
            }
            _logger.LogInformation("Token 中的 userId claim: {UserId}", userIdClaim);
            
            if (!string.IsNullOrEmpty(userIdClaim) && long.TryParse(userIdClaim, out var userId))
            {
                _logger.LogInformation("SSE Token 验证成功, UserId: {UserId}", userId);
                return userId;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SSE Token 验证异常");
        }
        return null;
    }

    /// <summary>
    /// 向指定用户推送消息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="eventType">事件类型</param>
    /// <param name="data">消息数据</param>
    public static async Task NotifyUserAsync(long userId, string eventType, object data)
    {
        List<HttpResponse> connections;
        lock (_lock)
        {
            if (!_connections.TryGetValue(userId, out connections) || connections.Count == 0)
            {
                _staticLogger?.LogWarning("SSE推送失败：用户 {UserId} 没有在线连接（事件类型：{EventType}）", userId, eventType);
                return;
            }
            connections = connections.ToList();
        }

        var json = JsonSerializer.Serialize(data);
        var message = $"event: {eventType}\ndata: {json}\n\n";
        var bytes = Encoding.UTF8.GetBytes(message);

        int successCount = 0;
        int failCount = 0;

        foreach (var response in connections)
        {
            try
            {
                await response.Body.WriteAsync(bytes);
                await response.Body.FlushAsync();
                successCount++;
            }
            catch (Exception ex)
            {
                failCount++;
                _staticLogger?.LogWarning(ex, "SSE推送到单个连接失败（用户：{UserId}，事件：{EventType}）", userId, eventType);
            }
        }

        _staticLogger?.LogInformation(
            "SSE推送完成：用户={UserId}, 事件={EventType}, 成功={SuccessCount}, 失败={FailCount}, 总连接数={TotalCount}",
            userId, eventType, successCount, failCount, connections.Count);
    }

    /// <summary>
    /// 向所有在线用户推送消息
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <param name="data">消息数据</param>
    public static async Task NotifyAllAsync(string eventType, object data)
    {
        List<HttpResponse> allConnections;
        lock (_lock)
        {
            allConnections = _connections.Values
                .SelectMany(list => list)
                .ToList();
        }

        if (allConnections.Count == 0)
        {
            return;
        }

        var json = JsonSerializer.Serialize(data);
        var message = $"event: {eventType}\ndata: {json}\n\n";
        var bytes = Encoding.UTF8.GetBytes(message);

        foreach (var response in allConnections)
        {
            try
            {
                await response.Body.WriteAsync(bytes);
                await response.Body.FlushAsync();
            }
            catch
            {
            }
        }
    }

    private async Task SendEventAsync(HttpResponse response, string eventType, object data)
    {
        var json = JsonSerializer.Serialize(data);
        var message = $"event: {eventType}\ndata: {json}\n\n";
        var bytes = Encoding.UTF8.GetBytes(message);
        await response.Body.WriteAsync(bytes);
        await response.Body.FlushAsync();
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return null;
        }
        return long.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
