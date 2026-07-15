using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models;

namespace qisu_server.Services;

/// <summary>
/// 操作日志服务接口，定义各类操作日志的记录方法
/// </summary>
public interface IOperationLogService
{
    /// <summary>
    /// 通用日志记录方法，所有其他日志方法最终都通过此方法写入数据库
    /// </summary>
    /// <param name="request">日志请求对象，包含日志的所有字段信息</param>
    Task LogAsync(OperationLogRequest request);

    /// <summary>
    /// 记录用户登录日志
    /// </summary>
    /// <param name="userId">登录用户ID，登录失败时可能为null</param>
    /// <param name="username">登录用户名</param>
    /// <param name="ipAddress">登录IP地址</param>
    /// <param name="browser">浏览器信息</param>
    /// <param name="os">操作系统信息</param>
    /// <param name="success">是否登录成功</param>
    /// <param name="errorMessage">失败时的错误信息</param>
    Task LogLoginAsync(long? userId, string? username, string ipAddress, string? browser, string? os, bool success, string? errorMessage = null);

    /// <summary>
    /// 记录用户退出登录日志
    /// </summary>
    /// <param name="userId">退出用户ID</param>
    /// <param name="username">退出用户名</param>
    /// <param name="ipAddress">请求IP地址</param>
    Task LogLogoutAsync(long? userId, string? username, string ipAddress);

    /// <summary>
    /// 记录用户注册日志
    /// </summary>
    /// <param name="userId">注册用户ID，注册失败时可能为null</param>
    /// <param name="username">注册用户名</param>
    /// <param name="phone">注册手机号</param>
    /// <param name="ipAddress">注册IP地址</param>
    /// <param name="success">是否注册成功</param>
    /// <param name="errorMessage">失败时的错误信息</param>
    Task LogRegisterAsync(long? userId, string? username, string phone, string ipAddress, bool success, string? errorMessage = null);

    /// <summary>
    /// 记录密码重置日志
    /// </summary>
    /// <param name="phone">重置密码的手机号</param>
    /// <param name="ipAddress">请求IP地址</param>
    /// <param name="success">是否重置成功</param>
    /// <param name="errorMessage">失败时的错误信息</param>
    Task LogPasswordResetAsync(string phone, string ipAddress, bool success, string? errorMessage = null);

    /// <summary>
    /// 记录审核操作日志，如房东申请审核、房源审核等
    /// </summary>
    /// <param name="auditorId">审核人ID</param>
    /// <param name="auditorName">审核人名称</param>
    /// <param name="targetType">审核目标类型，如"房东申请"、"房源"等</param>
    /// <param name="targetId">审核目标ID</param>
    /// <param name="action">审核动作，如"通过"、"拒绝"等</param>
    /// <param name="description">操作描述</param>
    /// <param name="ipAddress">审核人IP地址</param>
    /// <param name="success">是否操作成功</param>
    /// <param name="errorMessage">失败时的错误信息</param>
    Task LogAuditAsync(long? auditorId, string? auditorName, string targetType, long targetId, string action, string description, string ipAddress, bool success, string? errorMessage = null);

    /// <summary>
    /// 记录用户管理操作日志，如管理员封禁/解封用户等
    /// </summary>
    /// <param name="operatorId">操作人ID</param>
    /// <param name="operatorName">操作人名称</param>
    /// <param name="action">操作动作，如"封禁"、"解封"、"删除"等</param>
    /// <param name="targetUserId">被操作的目标用户ID</param>
    /// <param name="description">操作描述</param>
    /// <param name="ipAddress">操作人IP地址</param>
    /// <param name="success">是否操作成功</param>
    /// <param name="errorMessage">失败时的错误信息</param>
    Task LogUserManageAsync(long? operatorId, string? operatorName, string action, long targetUserId, string description, string ipAddress, bool success, string? errorMessage = null);

    /// <summary>
    /// 记录数据变更日志，用于记录系统中的数据增删改操作
    /// </summary>
    /// <param name="operatorId">操作人ID</param>
    /// <param name="operatorName">操作人名称</param>
    /// <param name="type">日志类型</param>
    /// <param name="description">操作描述</param>
    /// <param name="ipAddress">操作人IP地址</param>
    /// <param name="requestUrl">请求URL</param>
    /// <param name="requestMethod">请求方法</param>
    /// <param name="requestParams">请求参数</param>
    /// <param name="success">是否操作成功</param>
    /// <param name="errorMessage">失败时的错误信息</param>
    Task LogDataChangeAsync(long? operatorId, string? operatorName, string type, string description, string ipAddress, string? requestUrl = null, string? requestMethod = null, string? requestParams = null, bool success = true, string? errorMessage = null);
}

/// <summary>
/// 操作日志请求对象，用于传递日志记录所需的各项信息
/// </summary>
public class OperationLogRequest
{
    /// <summary>
    /// 操作类型，默认为"system"
    /// </summary>
    public string Type { get; set; } = "system";

    /// <summary>
    /// 操作人用户ID
    /// </summary>
    public long? OperatorId { get; set; }

    /// <summary>
    /// 操作人用户名
    /// </summary>
    public string? OperatorName { get; set; }

    /// <summary>
    /// 操作描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 操作者IP地址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// IP地址对应的地理位置
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// 浏览器信息
    /// </summary>
    public string? Browser { get; set; }

    /// <summary>
    /// 操作系统信息
    /// </summary>
    public string? Os { get; set; }

    /// <summary>
    /// 请求URL路径
    /// </summary>
    public string? RequestUrl { get; set; }

    /// <summary>
    /// 请求HTTP方法
    /// </summary>
    public string? RequestMethod { get; set; }

    /// <summary>
    /// 请求参数
    /// </summary>
    public string? RequestParams { get; set; }

    /// <summary>
    /// HTTP响应状态码
    /// </summary>
    public int? ResponseCode { get; set; }

    /// <summary>
    /// 操作状态，默认为"success"
    /// </summary>
    public string Status { get; set; } = "success";

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 请求耗时（毫秒）
    /// </summary>
    public int? Duration { get; set; }
}

/// <summary>
/// 操作日志服务实现，负责将各类操作日志异步写入数据库
/// 使用信号量控制并发写入数量，通过IP定位服务获取地理位置
/// </summary>
public class OperationLogService : IOperationLogService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OperationLogService> _logger;
    private readonly IIpLocationService _ipLocationService;

    /// <summary>
    /// 信号量，限制最多5个并发日志写入操作，防止数据库连接池耗尽
    /// </summary>
    private readonly SemaphoreSlim _semaphore = new(5, 5);

    public OperationLogService(IServiceScopeFactory scopeFactory, ILogger<OperationLogService> logger, IIpLocationService ipLocationService)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _ipLocationService = ipLocationService;
    }

    /// <summary>
    /// 通用日志记录方法，异步将日志写入数据库
    /// 使用 Task.Run 实现真正的异步（fire-and-forget），不阻塞调用方
    /// 通过信号量控制并发数，防止大量日志写入导致数据库压力过大
    /// </summary>
    public async Task LogAsync(OperationLogRequest request)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await _semaphore.WaitAsync();
                try
                {
                    // 由于是后台任务，需要创建新的作用域来获取DbContext，避免跨请求复用DbContext
                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var ipAddress = IpLocationService.NormalizeIpAddress(request.IpAddress);
                    var location = request.Location;

                    if (!string.IsNullOrEmpty(ipAddress) && ipAddress != "unknown")
                    {
                        try
                        {
                            var (actualIp, actualLocation) = await _ipLocationService.GetLocationWithIpAsync(ipAddress);
                            if (!string.IsNullOrEmpty(actualIp))
                            {
                                ipAddress = actualIp;
                            }
                            if (string.IsNullOrEmpty(location) && !string.IsNullOrEmpty(actualLocation))
                            {
                                location = actualLocation;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "获取IP地理位置失败，跳过");
                            if (IpLocationService.IsLocalIpAddress(ipAddress))
                            {
                                location = "本地网络";
                            }
                        }
                    }

                    var log = new OperationLog
                    {
                        Type = request.Type,
                        OperatorId = request.OperatorId,
                        OperatorName = request.OperatorName,
                        Description = request.Description,
                        IpAddress = ipAddress,
                        Location = location,
                        Browser = request.Browser,
                        Os = request.Os,
                        RequestUrl = request.RequestUrl,
                        RequestMethod = request.RequestMethod,
                        RequestParams = request.RequestParams,
                        ResponseCode = request.ResponseCode,
                        Status = request.Status,
                        ErrorMessage = request.ErrorMessage,
                        Duration = request.Duration,
                        CreatedAt = DateTime.Now
                    };

                    context.OperationLogs.Add(log);
                    await context.SaveChangesAsync();
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                // 日志记录失败不应影响业务流程，仅记录错误日志
                _logger.LogError(ex, "记录操作日志失败");
            }
        });
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// 记录用户登录日志
    /// </summary>
    public async Task LogLoginAsync(long? userId, string? username, string ipAddress, string? browser, string? os, bool success, string? errorMessage = null)
    {
        await LogAsync(new OperationLogRequest
        {
            Type = "login",
            OperatorId = userId,
            OperatorName = username,
            Description = success ? $"用户 [{username}] 登录成功" : $"用户 [{username}] 登录失败",
            IpAddress = ipAddress,
            Browser = browser,
            Os = os,
            Status = success ? "success" : "fail",
            ErrorMessage = errorMessage
        });
    }

    /// <summary>
    /// 记录用户退出登录日志
    /// </summary>
    public async Task LogLogoutAsync(long? userId, string? username, string ipAddress)
    {
        await LogAsync(new OperationLogRequest
        {
            Type = "logout",
            OperatorId = userId,
            OperatorName = username,
            Description = $"用户 [{username}] 退出登录",
            IpAddress = ipAddress,
            Status = "success"
        });
    }

    /// <summary>
    /// 记录用户注册日志
    /// </summary>
    public async Task LogRegisterAsync(long? userId, string? username, string phone, string ipAddress, bool success, string? errorMessage = null)
    {
        await LogAsync(new OperationLogRequest
        {
            Type = "register",
            OperatorId = userId,
            OperatorName = username,
            Description = success ? $"用户 [{username}] 注册成功" : $"用户 [{username}] 注册失败",
            IpAddress = ipAddress,
            Status = success ? "success" : "fail",
            ErrorMessage = errorMessage
        });
    }

    /// <summary>
    /// 记录密码重置日志
    /// </summary>
    public async Task LogPasswordResetAsync(string phone, string ipAddress, bool success, string? errorMessage = null)
    {
        await LogAsync(new OperationLogRequest
        {
            Type = "security",
            Description = success ? $"手机号 [{phone}] 密码重置成功" : $"手机号 [{phone}] 密码重置失败",
            IpAddress = ipAddress,
            Status = success ? "success" : "fail",
            ErrorMessage = errorMessage
        });
    }

    /// <summary>
    /// 记录审核操作日志
    /// </summary>
    public async Task LogAuditAsync(long? auditorId, string? auditorName, string targetType, long targetId, string action, string description, string ipAddress, bool success, string? errorMessage = null)
    {
        await LogAsync(new OperationLogRequest
        {
            Type = "audit",
            OperatorId = auditorId,
            OperatorName = auditorName,
            Description = $"[{auditorName}] {action} {targetType}(ID:{targetId}): {description}",
            IpAddress = ipAddress,
            Status = success ? "success" : "fail",
            ErrorMessage = errorMessage
        });
    }

    /// <summary>
    /// 记录用户管理操作日志
    /// </summary>
    public async Task LogUserManageAsync(long? operatorId, string? operatorName, string action, long targetUserId, string description, string ipAddress, bool success, string? errorMessage = null)
    {
        await LogAsync(new OperationLogRequest
        {
            Type = "user_manage",
            OperatorId = operatorId,
            OperatorName = operatorName,
            Description = $"[{operatorName}] {action} 用户(ID:{targetUserId}): {description}",
            IpAddress = ipAddress,
            Status = success ? "success" : "fail",
            ErrorMessage = errorMessage
        });
    }

    /// <summary>
    /// 记录数据变更日志
    /// </summary>
    public async Task LogDataChangeAsync(long? operatorId, string? operatorName, string type, string description, string ipAddress, string? requestUrl = null, string? requestMethod = null, string? requestParams = null, bool success = true, string? errorMessage = null)
    {
        await LogAsync(new OperationLogRequest
        {
            Type = type,
            OperatorId = operatorId,
            OperatorName = operatorName,
            Description = description,
            IpAddress = ipAddress,
            RequestUrl = requestUrl,
            RequestMethod = requestMethod,
            RequestParams = requestParams,
            Status = success ? "success" : "fail",
            ErrorMessage = errorMessage
        });
    }
}
