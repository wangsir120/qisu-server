using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using qisu_server.Data;
using qisu_server.Models;
using qisu_server.Services;

namespace qisu_server.Middleware;

public class WebSocketMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<WebSocketMiddleware> _logger;
    
    private static readonly Dictionary<string, WebSocket> _connections = new();
    private static readonly object _lock = new();

    public WebSocketMiddleware(RequestDelegate next, ILogger<WebSocketMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        if (context.Request.Path == "/ws/chat")
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var token = context.Request.Query["token"].ToString();
                (long? userId, bool isAdmin, string? error) = ValidateToken(token, serviceProvider);

                if (userId == null)
                {
                    _logger.LogWarning("WebSocket 认证失败: {Error}", error ?? "未知错误");
                    context.Response.StatusCode = 401;
                    return;
                }

                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                
                var connectionKey = isAdmin ? $"admin_{userId.Value}" : $"user_{userId.Value}";
                
                lock (_lock)
                {
                    _connections[connectionKey] = webSocket;
                }

                _logger.LogInformation("WebSocket 连接建立: {ConnectionKey}", connectionKey);

                try
                {
                    await SendAsync(webSocket, "connected", new { message = "WebSocket 连接成功", userId, isAdmin, connectionKey });
                    await ReceiveAsync(webSocket, userId.Value, isAdmin, serviceProvider);
                }
                catch (WebSocketException ex)
                {
                    _logger.LogError(ex, "WebSocket 错误: {ConnectionKey}", connectionKey);
                }
                finally
                {
                    lock (_lock)
                    {
                        _connections.Remove(connectionKey);
                    }
                    
                    if (webSocket.State == WebSocketState.Open)
                    {
                        await webSocket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "连接关闭",
                            CancellationToken.None);
                    }
                    
                    _logger.LogInformation("WebSocket 连接关闭: {ConnectionKey}", connectionKey);
                }
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }
        else
        {
            await _next(context);
        }
    }

    private (long? userId, bool isAdmin, string? error) ValidateToken(string token, IServiceProvider serviceProvider)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
            {
                return (null, false, "Token 为空");
            }

            var jwtService = serviceProvider.GetService<IJwtService>();
            if (jwtService == null)
            {
                return (null, false, "JwtService 未注册");
            }

            var principal = jwtService.ValidateToken(token);
            if (principal == null)
            {
                return (null, false, "Token 验证失败");
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

            if (!string.IsNullOrEmpty(userIdClaim) && long.TryParse(userIdClaim, out var userId))
            {
                var roleClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                var isAdmin = roleClaim == "admin" || roleClaim == "super_admin";
                
                return (userId, isAdmin, null);
            }
            
            return (null, false, "无法获取用户ID");
        }
        catch (SecurityTokenExpiredException)
        {
            return (null, false, "Token 已过期，请重新登录");
        }
        catch (SecurityTokenException ex)
        {
            return (null, false, $"Token 无效: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token 验证异常");
            return (null, false, "Token 验证异常");
        }
    }

    private async Task ReceiveAsync(WebSocket webSocket, long userId, bool isAdmin, IServiceProvider serviceProvider)
    {
        var buffer = new byte[1024 * 4];

        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                _logger.LogInformation("收到消息: UserId={UserId}, IsAdmin={IsAdmin}, Message={Message}", userId, isAdmin, message);

                try
                {
                    var data = JsonSerializer.Deserialize<WebSocketMessage>(message);
                    if (data != null)
                    {
                        await HandleMessageAsync(webSocket, userId, isAdmin, data, serviceProvider);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "消息解析失败");
                }
            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                break;
            }
        }
    }

    private async Task HandleMessageAsync(WebSocket webSocket, long userId, bool isAdmin, WebSocketMessage message, IServiceProvider serviceProvider)
    {
        switch (message.Type)
        {
            case "heartbeat":
                await SendAsync(webSocket, "heartbeat", new { time = DateTime.Now });
                break;

            case "chat":
                if (isAdmin)
                {
                    await ProcessAdminChatMessageAsync(userId, message.Payload, serviceProvider);
                }
                else
                {
                    await ProcessUserChatMessageAsync(userId, message.Payload, serviceProvider);
                }
                break;

            case "typing":
                break;

            default:
                _logger.LogWarning("未知消息类型: {Type}", message.Type);
                break;
        }
    }

    private async Task ProcessUserChatMessageAsync(long userId, object? payload, IServiceProvider serviceProvider)
    {
        var content = string.Empty;
        var messageType = "text";
        
        if (payload is JsonElement element)
        {
            if (element.TryGetProperty("content", out var contentProp))
            {
                content = contentProp.GetString() ?? string.Empty;
            }
            if (element.TryGetProperty("messageType", out var typeProp))
            {
                messageType = typeProp.GetString() ?? "text";
            }
        }

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var conversation = await context.ChatConversations
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == "active");

        string conversationId;
        if (conversation == null)
        {
            conversationId = Guid.NewGuid().ToString("N");
            conversation = new ChatConversation
            {
                ConversationId = conversationId,
                UserId = userId,
                AdminId = 1001,
                LastMessage = messageType == "image" ? "[图片]" : content,
                LastMessageTime = DateTime.Now,
                Status = "active"
            };
            context.ChatConversations.Add(conversation);
        }
        else
        {
            conversationId = conversation.ConversationId;
            conversation.LastMessage = messageType == "image" ? "[图片]" : content;
            conversation.LastMessageTime = DateTime.Now;
            conversation.UnreadCount++;
        }

        var message = new ChatMessage
        {
            ConversationId = conversationId,
            SenderId = userId,
            ReceiverId = 1001,
            Content = content,
            MessageType = messageType,
            IsRead = false,
            CreatedAt = DateTime.Now
        };

        context.ChatMessages.Add(message);
        await context.SaveChangesAsync();

        var user = await context.Users.FindAsync(userId);
        var senderName = user?.Nickname ?? user?.Username ?? user?.Phone ?? "用户";
        
        _logger.LogInformation("发送消息给管理员: AdminId=1001, ConversationId={ConversationId}, UnreadCount={UnreadCount}, SenderName={SenderName}, MessageType={MessageType}", 
            conversationId, conversation.UnreadCount, senderName, messageType);
        
        await SendToAdminAsync(1001, "chat", new
        {
            id = message.Id,
            conversationId = message.ConversationId,
            senderId = userId,
            senderName = senderName,
            senderAvatar = user?.Avatar,
            content = content,
            messageType = messageType,
            timestamp = message.CreatedAt,
            isAdmin = false
        });
    }

    private async Task ProcessAdminChatMessageAsync(long adminId, object? payload, IServiceProvider serviceProvider)
    {
        var content = string.Empty;
        var receiverId = 0L;
        var conversationId = string.Empty;
        var messageType = "text";

        if (payload is JsonElement element)
        {
            if (element.TryGetProperty("content", out var contentProp))
            {
                content = contentProp.GetString() ?? string.Empty;
            }
            if (element.TryGetProperty("receiverId", out var receiverProp))
            {
                receiverId = receiverProp.GetInt64();
            }
            if (element.TryGetProperty("conversationId", out var convProp))
            {
                conversationId = convProp.GetString() ?? string.Empty;
            }
            if (element.TryGetProperty("messageType", out var typeProp))
            {
                messageType = typeProp.GetString() ?? "text";
            }
        }

        _logger.LogInformation("管理员发送消息: AdminId={AdminId}, ReceiverId={ReceiverId}, ConversationId={ConversationId}, MessageType={MessageType}", 
            adminId, receiverId, conversationId, messageType);

        if (receiverId == 0 || string.IsNullOrEmpty(content))
        {
            _logger.LogWarning("消息参数无效: ReceiverId={ReceiverId}, Content={Content}", receiverId, content);
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var message = new ChatMessage
        {
            ConversationId = conversationId,
            SenderId = adminId,
            ReceiverId = receiverId,
            Content = content,
            MessageType = messageType,
            IsRead = false,
            CreatedAt = DateTime.Now
        };

        _logger.LogInformation("保存管理员消息: SenderId={SenderId}, ReceiverId={ReceiverId}, MessageType={MessageType}", message.SenderId, message.ReceiverId, messageType);

        context.ChatMessages.Add(message);

        var conversation = await context.ChatConversations
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId);
        if (conversation != null)
        {
            conversation.LastMessage = messageType == "image" ? "[图片]" : content;
            conversation.LastMessageTime = DateTime.Now;
        }

        await context.SaveChangesAsync();

        var admin = await context.Admins.FindAsync(adminId);
        
        await SendToUserAsync(receiverId, "chat", new
        {
            id = message.Id,
            conversationId = message.ConversationId,
            senderId = adminId,
            senderName = admin?.Name ?? admin?.Username ?? "客服",
            senderAvatar = admin?.Avatar,
            content = content,
            messageType = messageType,
            timestamp = message.CreatedAt,
            isAdmin = true
        });
    }

    private static async Task SendAsync(WebSocket webSocket, string type, object data)
    {
        if (webSocket.State != WebSocketState.Open)
            return;

        var message = new
        {
            type,
            payload = data
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var json = JsonSerializer.Serialize(message, jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        await webSocket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }

    public static async Task SendToUserAsync(long userId, string type, object data)
    {
        WebSocket? webSocket;
        var connectionKey = $"user_{userId}";
        lock (_lock)
        {
            _connections.TryGetValue(connectionKey, out webSocket);
        }

        if (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            await SendAsync(webSocket, type, data);
        }
    }

    public static async Task SendToAdminAsync(long adminId, string type, object data)
    {
        WebSocket? webSocket;
        var connectionKey = $"admin_{adminId}";
        lock (_lock)
        {
            _connections.TryGetValue(connectionKey, out webSocket);
        }

        if (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            await SendAsync(webSocket, type, data);
        }
        else
        {
            Console.WriteLine($"管理员 {adminId} 不在线或连接已关闭，无法发送消息");
        }
    }

    public static async Task BroadcastAsync(string type, object data)
    {
        List<WebSocket> connections;
        lock (_lock)
        {
            connections = _connections.Values.ToList();
        }

        foreach (var webSocket in connections)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                await SendAsync(webSocket, type, data);
            }
        }
    }
}

public class WebSocketMessage
{
    [System.Text.Json.Serialization.JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("payload")]
    public object? Payload { get; set; }
}
