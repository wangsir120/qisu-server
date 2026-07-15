using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using qisu_server.Services;

namespace qisu_server.Middleware;

/// <summary>
/// 操作日志中间件，自动拦截HTTP请求并记录操作日志
/// 仅对 POST、PUT、DELETE、PATCH 方法的请求进行日志记录
/// 忽略登录、注册、验证码等公共接口，避免产生大量无用日志
/// </summary>
public class OperationLogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<OperationLogMiddleware> _logger;

    /// <summary>
    /// 不记录日志的路径前缀列表
    /// 这些路径通常是高频、低价值的操作（如登录、验证码、公共接口等），记录日志意义不大
    /// </summary>
    private static readonly string[] _ignorePaths = new[]
    {
        "/api/admin/logs",          // 日志查询接口本身，避免递归记录
        "/api/auth/captcha",        // 验证码获取
        "/api/auth/send-sms-code",  // 发送短信验证码
        "/api/auth/verify-sms-code",// 验证短信验证码
        "/api/auth/login",          // 用户登录（由LogLoginAsync单独记录）
        "/api/auth/admin/login",    // 管理员登录（由LogLoginAsync单独记录）
        "/api/auth/host/login",     // 房东登录（由LogLoginAsync单独记录）
        "/api/auth/logout",         // 退出登录（由LogLogoutAsync单独记录）
        "/api/auth/register",       // 用户注册（由LogRegisterAsync单独记录）
        "/api/auth/reset-password", // 密码重置（由LogPasswordResetAsync单独记录）
        "/api/public",              // 公共接口，无需记录
        "/api/sse",                 // SSE推送接口，长连接不适合记录
        "/swagger",                 // Swagger文档
        "/favicon.ico"              // 网站图标
    };

    /// <summary>
    /// 需要记录日志的HTTP方法，仅记录数据变更操作
    /// GET请求通常只是查询，不涉及数据变更，因此不记录
    /// </summary>
    private static readonly string[] _logMethods = new[] { "POST", "PUT", "DELETE", "PATCH" };

    public OperationLogMiddleware(RequestDelegate next, ILogger<OperationLogMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// 中间件核心处理方法
    /// 流程：1.判断是否需要记录 → 2.记录请求体 → 3.执行后续中间件 → 4.记录响应信息并写入日志
    /// 使用 MemoryStream 捕获响应体，确保不影响正常的响应输出
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <param name="logService">操作日志服务，通过DI注入</param>
    public async Task InvokeAsync(HttpContext context, IOperationLogService logService)
    {
        var path = context.Request.Path.Value ?? "";
        
        // 不需要记录日志的请求直接放行
        if (ShouldIgnore(path) || !_logMethods.Contains(context.Request.Method, StringComparer.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // 计时器，用于记录请求耗时
        var stopwatch = Stopwatch.StartNew();
        // 读取请求体内容（需要在请求管道执行前读取，否则流会被消费）
        var requestBody = await ReadRequestBodyAsync(context.Request);
        // 保存原始响应流，用于后续恢复
        var originalBodyStream = context.Response.Body;
        
        // 用内存流替换响应流，以便在不影响客户端的情况下读取响应
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        // 捕获后续中间件可能抛出的异常，用于记录失败日志
        Exception? caughtException = null;
        
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            caughtException = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            
            // 将内存流中的响应数据复制回原始响应流，确保客户端能正常接收响应
            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;

            try
            {
                // 从JWT Claims中提取当前用户信息
                var (userId, username) = GetUserFromContext(context);
                // 从User-Agent中解析浏览器和操作系统
                var (browser, os) = ParseUserAgent(context.Request.Headers.UserAgent);
                // 获取客户端真实IP地址（支持代理）
                var ipAddress = GetClientIpAddress(context);

                // 根据请求路径判断日志类型
                var logType = GetLogType(path);
                // 根据请求方法和响应状态生成操作描述
                var description = GenerateDescription(context.Request.Method, path, context.Response.StatusCode);

                await logService.LogAsync(new OperationLogRequest
                {
                    Type = logType,
                    OperatorId = userId,
                    OperatorName = username,
                    Description = description,
                    IpAddress = ipAddress,
                    Browser = browser,
                    Os = os,
                    RequestUrl = path,
                    RequestMethod = context.Request.Method,
                    RequestParams = TruncateString(requestBody, 2000),
                    ResponseCode = context.Response.StatusCode,
                    Status = caughtException != null || context.Response.StatusCode >= 400 ? "fail" : "success",
                    ErrorMessage = caughtException?.Message,
                    Duration = (int)stopwatch.ElapsedMilliseconds
                });
            }
            catch (Exception ex)
            {
                // 日志记录失败不应影响业务流程
                _logger.LogError(ex, "记录操作日志失败");
            }
        }
    }

    /// <summary>
    /// 判断请求路径是否应被忽略
    /// 使用前缀匹配，即路径以忽略列表中的任一前缀开头则忽略
    /// </summary>
    /// <param name="path">请求路径</param>
    /// <returns>需要忽略返回true，否则返回false</returns>
    private bool ShouldIgnore(string path)
    {
        return _ignorePaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 根据请求路径判断日志类型
    /// 路径中包含特定关键词时返回对应类型，否则默认为"system"
    /// </summary>
    /// <param name="path">请求路径</param>
    /// <returns>日志类型字符串</returns>
    private string GetLogType(string path)
    {
        if (path.Contains("/host-application") || path.Contains("/host-apply"))
            return "audit";
        if (path.Contains("/user") && path.Contains("/manage"))
            return "user_manage";
        if (path.Contains("/announcement"))
            return "announcement";
        if (path.Contains("/banner"))
            return "banner";
        return "system";
    }

    /// <summary>
    /// 根据HTTP方法和响应状态码生成操作描述
    /// 如："新增 properties 成功"、"删除 123 失败"
    /// </summary>
    /// <param name="method">HTTP方法</param>
    /// <param name="path">请求路径</param>
    /// <param name="statusCode">HTTP响应状态码</param>
    /// <returns>操作描述字符串</returns>
    private string GenerateDescription(string method, string path, int statusCode)
    {
        var action = method.ToUpper() switch
        {
            "POST" => "新增",
            "PUT" => "修改",
            "DELETE" => "删除",
            "PATCH" => "更新",
            _ => "操作"
        };

        // 取路径最后一段作为资源名称
        var resource = path.Split('/').LastOrDefault() ?? "资源";
        var status = statusCode >= 200 && statusCode < 300 ? "成功" : "失败";

        return $"{action} {resource} {status}";
    }

    /// <summary>
    /// 从HttpContext的JWT Claims中提取当前用户信息
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>用户ID和用户名的元组，未登录时均为null</returns>
    private (long? userId, string? username) GetUserFromContext(HttpContext context)
    {
        var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usernameClaim = context.User?.FindFirst(ClaimTypes.Name)?.Value;

        long? userId = null;
        if (long.TryParse(userIdClaim, out var parsedId))
        {
            userId = parsedId;
        }

        return (userId, usernameClaim);
    }

    /// <summary>
    /// 获取客户端真实IP地址
    /// 优先从代理头（X-Forwarded-For、X-Real-IP）中获取，兼容Nginx等反向代理场景
    /// X-Forwarded-For可能包含多个IP（经过多级代理），取第一个即客户端真实IP
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>客户端IP地址，无法获取时返回"unknown"</returns>
    private string GetClientIpAddress(HttpContext context)
    {
        string? ip = null;

        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            ip = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim())
                            .FirstOrDefault(s => !string.IsNullOrEmpty(s) && !s.Equals("unknown", StringComparison.OrdinalIgnoreCase));
        }

        if (string.IsNullOrEmpty(ip))
        {
            ip = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        }

        if (string.IsNullOrEmpty(ip))
        {
            ip = context.Connection.RemoteIpAddress?.ToString();
        }

        return IpLocationService.NormalizeIpAddress(ip);
    }

    /// <summary>
    /// 解析User-Agent字符串，提取浏览器和操作系统信息
    /// 支持主流浏览器：Chrome、Safari、Firefox、Edge、IE
    /// 支持主流操作系统：Windows 7/8/8.1/10、macOS、Android、iOS、Linux
    /// </summary>
    /// <param name="userAgent">User-Agent请求头字符串</param>
    /// <returns>浏览器和操作系统信息的元组</returns>
    private (string? browser, string? os) ParseUserAgent(string? userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
        {
            return (null, null);
        }

        var ua = userAgent.ToString();
        string? browser = null;
        string? os = null;

        // 浏览器判断（注意顺序：Edg必须在Chrome之前判断，因为Edge的UA也包含Chrome）
        if (ua.Contains("Chrome") && !ua.Contains("Edg"))
            browser = "Chrome";
        else if (ua.Contains("Safari") && !ua.Contains("Chrome"))
            browser = "Safari";
        else if (ua.Contains("Firefox"))
            browser = "Firefox";
        else if (ua.Contains("Edg"))
            browser = "Edge";
        else if (ua.Contains("MSIE") || ua.Contains("Trident"))
            browser = "IE";

        // 操作系统判断
        if (ua.Contains("Windows NT 10"))
            os = "Windows 10";
        else if (ua.Contains("Windows NT 6.3"))
            os = "Windows 8.1";
        else if (ua.Contains("Windows NT 6.2"))
            os = "Windows 8";
        else if (ua.Contains("Windows NT 6.1"))
            os = "Windows 7";
        else if (ua.Contains("Mac OS X"))
            os = "macOS";
        else if (ua.Contains("Android"))
            os = "Android";
        else if (ua.Contains("iPhone") || ua.Contains("iPad"))
            os = "iOS";
        else if (ua.Contains("Linux"))
            os = "Linux";

        return (browser, os);
    }

    /// <summary>
    /// 读取HTTP请求体内容
    /// 使用EnableBuffering允许多次读取请求流，读取后将流位置重置为0
    /// 请求体内容会被截断到最大长度，避免存储过大的请求参数
    /// </summary>
    /// <param name="request">HTTP请求对象</param>
    /// <returns>请求体字符串，无内容时返回null</returns>
    private async Task<string?> ReadRequestBodyAsync(HttpRequest request)
    {
        try
        {
            if (request.ContentLength == null || request.ContentLength == 0)
            {
                return null;
            }

            // 启用请求体缓冲，允许多次读取
            request.EnableBuffering();
            
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            // 重置流位置，确保后续中间件可以正常读取请求体
            request.Body.Position = 0;

            return TruncateString(body, 2000);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 截断字符串到指定最大长度，超出部分用"..."标识
    /// 防止过长的请求参数占用过多数据库存储空间
    /// </summary>
    /// <param name="value">原始字符串</param>
    /// <param name="maxLength">最大长度</param>
    /// <returns>截断后的字符串，null或空字符串时返回null</returns>
    private string? TruncateString(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        return value.Length > maxLength ? value.Substring(0, maxLength) + "..." : value;
    }
}

/// <summary>
/// 操作日志中间件扩展方法，提供更优雅的注册方式
/// 使用方式：app.UseOperationLog()
/// </summary>
public static class OperationLogMiddlewareExtensions
{
    /// <summary>
    /// 注册操作日志中间件到应用管道
    /// </summary>
    /// <param name="builder">应用构建器</param>
    /// <returns>应用构建器（支持链式调用）</returns>
    public static IApplicationBuilder UseOperationLog(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<OperationLogMiddleware>();
    }
}
