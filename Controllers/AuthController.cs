using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using qisu_server.Models.DTOs;
using qisu_server.Services;

namespace qisu_server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ICaptchaService _captchaService;
    private readonly IUserService _userService;
    private readonly ISmsService _smsService;
    private readonly IOperationLogService _logService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ICaptchaService captchaService, IUserService userService, ISmsService smsService, IOperationLogService logService, ILogger<AuthController> logger)
    {
        _captchaService = captchaService;
        _userService = userService;
        _smsService = smsService;
        _logService = logService;
        _logger = logger;
    }

    /// <summary>
    /// 获取图形验证码
    /// </summary>
    /// <returns>验证码ID和验证码图片Base64</returns>
    [HttpGet("captcha")]
    public ApiResponse<CaptchaResponse> GetCaptcha()
    {
        var (captchaId, captchaText) = _captchaService.GenerateCaptcha();
        return ApiResponse<CaptchaResponse>.Ok(new CaptchaResponse
        {
            CaptchaId = captchaId,
            CaptchaText = captchaText
        });
    }

    /// <summary>
    /// C端用户登录
    /// </summary>
    /// <param name="request">登录请求，包含用户名和密码</param>
    /// <returns>登录结果，包含Token和用户信息</returns>
    [HttpPost("login")]
    public async Task<ApiResponse<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var ipAddress = GetClientIpAddress();
        var (browser, os) = ParseUserAgent();
        var requestUrl = Request.Path.Value;
        var requestMethod = Request.Method;

        if (string.IsNullOrEmpty(request.Username))
        {
            return ApiResponse<LoginResponse>.Fail("请输入用户名");
        }

        if (string.IsNullOrEmpty(request.Password))
        {
            return ApiResponse<LoginResponse>.Fail("请输入密码");
        }

        var result = await _userService.LoginAsync(request);
        stopwatch.Stop();

        await _logService.LogAsync(new OperationLogRequest
        {
            Type = "login",
            OperatorId = result.User?.Id,
            OperatorName = request.Username,
            Description = result.Success ? $"用户 [{request.Username}] 登录成功" : $"用户 [{request.Username}] 登录失败: {result.Message}",
            IpAddress = ipAddress,
            Browser = browser,
            Os = os,
            RequestUrl = requestUrl,
            RequestMethod = requestMethod,
            ResponseCode = result.Success ? 200 : 401,
            Status = result.Success ? "success" : "fail",
            ErrorMessage = result.Success ? null : result.Message,
            Duration = (int)stopwatch.ElapsedMilliseconds
        });
        
        if (result.Success)
        {
            return ApiResponse<LoginResponse>.Ok(result, result.Message);
        }
        
        return ApiResponse<LoginResponse>.Fail(result.Message, 401);
    }

    /// <summary>
    /// 管理员登录
    /// </summary>
    /// <param name="request">登录请求，包含用户名和密码</param>
    /// <returns>登录结果，包含Token和管理员信息</returns>
    [HttpPost("admin/login")]
    public async Task<ApiResponse<LoginResponse>> AdminLogin([FromBody] LoginRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var ipAddress = GetClientIpAddress();
        var (browser, os) = ParseUserAgent();
        var requestUrl = Request.Path.Value;
        var requestMethod = Request.Method;

        if (string.IsNullOrEmpty(request.Username))
        {
            return ApiResponse<LoginResponse>.Fail("请输入用户名");
        }

        if (string.IsNullOrEmpty(request.Password))
        {
            return ApiResponse<LoginResponse>.Fail("请输入密码");
        }

        var result = await _userService.AdminLoginAsync(request);
        stopwatch.Stop();

        await _logService.LogAsync(new OperationLogRequest
        {
            Type = "login",
            OperatorId = result.User?.Id,
            OperatorName = request.Username,
            Description = result.Success ? $"管理员 [{request.Username}] 登录成功" : $"管理员 [{request.Username}] 登录失败: {result.Message}",
            IpAddress = ipAddress,
            Browser = browser,
            Os = os,
            RequestUrl = requestUrl,
            RequestMethod = requestMethod,
            ResponseCode = result.Success ? 200 : 401,
            Status = result.Success ? "success" : "fail",
            ErrorMessage = result.Success ? null : result.Message,
            Duration = (int)stopwatch.ElapsedMilliseconds
        });
        
        if (result.Success)
        {
            return ApiResponse<LoginResponse>.Ok(result, result.Message);
        }
        
        return ApiResponse<LoginResponse>.Fail(result.Message, 401);
    }

    /// <summary>
    /// 房东登录
    /// </summary>
    /// <param name="request">登录请求，包含用户名和密码</param>
    /// <returns>登录结果，包含Token和房东信息</returns>
    [HttpPost("host/login")]
    public async Task<ApiResponse<LoginResponse>> HostLogin([FromBody] LoginRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var ipAddress = GetClientIpAddress();
        var (browser, os) = ParseUserAgent();
        var requestUrl = Request.Path.Value;
        var requestMethod = Request.Method;

        if (string.IsNullOrEmpty(request.Username))
        {
            return ApiResponse<LoginResponse>.Fail("请输入用户名");
        }

        if (string.IsNullOrEmpty(request.Password))
        {
            return ApiResponse<LoginResponse>.Fail("请输入密码");
        }

        var result = await _userService.HostLoginAsync(request);
        stopwatch.Stop();

        await _logService.LogAsync(new OperationLogRequest
        {
            Type = "login",
            OperatorId = result.User?.Id,
            OperatorName = request.Username,
            Description = result.Success ? $"房东 [{request.Username}] 登录成功" : $"房东 [{request.Username}] 登录失败: {result.Message}",
            IpAddress = ipAddress,
            Browser = browser,
            Os = os,
            RequestUrl = requestUrl,
            RequestMethod = requestMethod,
            ResponseCode = result.Success ? 200 : 401,
            Status = result.Success ? "success" : "fail",
            ErrorMessage = result.Success ? null : result.Message,
            Duration = (int)stopwatch.ElapsedMilliseconds
        });
        
        if (result.Success)
        {
            return ApiResponse<LoginResponse>.Ok(result, result.Message);
        }
        
        return ApiResponse<LoginResponse>.Fail(result.Message, 401);
    }

    /// <summary>
    /// 用户注册
    /// </summary>
    /// <param name="request">注册请求，包含用户名、密码、手机号和验证码</param>
    /// <returns>注册结果，包含Token和用户信息</returns>
    [HttpPost("register")]
    public async Task<ApiResponse<LoginResponse>> Register([FromBody] RegisterRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var ipAddress = GetClientIpAddress();
        var (browser, os) = ParseUserAgent();
        var requestUrl = Request.Path.Value;
        var requestMethod = Request.Method;

        if (string.IsNullOrEmpty(request.Username))
        {
            return ApiResponse<LoginResponse>.Fail("请输入用户名");
        }

        if (string.IsNullOrEmpty(request.Password))
        {
            return ApiResponse<LoginResponse>.Fail("请输入密码");
        }

        if (string.IsNullOrEmpty(request.Phone))
        {
            return ApiResponse<LoginResponse>.Fail("请输入手机号");
        }

        if (string.IsNullOrEmpty(request.VerifyCode))
        {
            return ApiResponse<LoginResponse>.Fail("请输入验证码");
        }

        var (verifySuccess, verifyMessage) = await _smsService.VerifyCodeAsync(request.Phone, request.VerifyCode);
        if (!verifySuccess)
        {
            return ApiResponse<LoginResponse>.Fail(verifyMessage);
        }

        var result = await _userService.RegisterAsync(request);
        stopwatch.Stop();

        await _logService.LogAsync(new OperationLogRequest
        {
            Type = "register",
            OperatorId = result.User?.Id,
            OperatorName = request.Username,
            Description = result.Success ? $"用户 [{request.Username}] 注册成功" : $"用户 [{request.Username}] 注册失败: {result.Message}",
            IpAddress = ipAddress,
            Browser = browser,
            Os = os,
            RequestUrl = requestUrl,
            RequestMethod = requestMethod,
            ResponseCode = result.Success ? 200 : 400,
            Status = result.Success ? "success" : "fail",
            ErrorMessage = result.Success ? null : result.Message,
            Duration = (int)stopwatch.ElapsedMilliseconds
        });
        
        if (result.Success)
        {
            return ApiResponse<LoginResponse>.Ok(result, result.Message);
        }
        
        return ApiResponse<LoginResponse>.Fail(result.Message);
    }

    /// <summary>
    /// 重置密码
    /// </summary>
    /// <param name="request">重置密码请求，包含手机号和新密码</param>
    /// <returns>重置结果</returns>
    [HttpPost("reset-password")]
    public async Task<ApiResponse<bool>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var ipAddress = GetClientIpAddress();
        var (browser, os) = ParseUserAgent();
        var requestUrl = Request.Path.Value;
        var requestMethod = Request.Method;

        if (string.IsNullOrEmpty(request.Phone))
        {
            return ApiResponse<bool>.Fail("请输入手机号");
        }

        if (string.IsNullOrEmpty(request.NewPassword))
        {
            return ApiResponse<bool>.Fail("请输入新密码");
        }

        var result = await _userService.ResetPasswordAsync(request);
        stopwatch.Stop();

        await _logService.LogAsync(new OperationLogRequest
        {
            Type = "security",
            Description = result.Success ? $"手机号 [{request.Phone}] 密码重置成功" : $"手机号 [{request.Phone}] 密码重置失败: {result.Message}",
            IpAddress = ipAddress,
            Browser = browser,
            Os = os,
            RequestUrl = requestUrl,
            RequestMethod = requestMethod,
            ResponseCode = result.Success ? 200 : 400,
            Status = result.Success ? "success" : "fail",
            ErrorMessage = result.Success ? null : result.Message,
            Duration = (int)stopwatch.ElapsedMilliseconds
        });
        
        return result;
    }

    /// <summary>
    /// 发送短信验证码
    /// </summary>
    /// <param name="request">发送请求，包含手机号</param>
    /// <returns>发送结果</returns>
    [HttpPost("send-sms-code")]
    public async Task<ApiResponse<bool>> SendSmsCode([FromBody] SendSmsCodeRequest request)
    {
        if (string.IsNullOrEmpty(request.Phone))
        {
            return ApiResponse<bool>.Fail("请输入手机号");
        }

        var (success, message, verifyCode) = await _smsService.SendVerifyCodeAsync(request.Phone);
        
        if (success)
        {
            return ApiResponse<bool>.Ok(true, message);
        }
        
        return ApiResponse<bool>.Fail(message);
    }

    /// <summary>
    /// 验证短信验证码
    /// </summary>
    /// <param name="request">验证请求，包含手机号和验证码</param>
    /// <returns>验证结果</returns>
    [HttpPost("verify-sms-code")]
    public async Task<ApiResponse<bool>> VerifySmsCode([FromBody] VerifySmsCodeRequest request)
    {
        if (string.IsNullOrEmpty(request.Phone))
        {
            return ApiResponse<bool>.Fail("请输入手机号");
        }

        if (string.IsNullOrEmpty(request.VerifyCode))
        {
            return ApiResponse<bool>.Fail("请输入验证码");
        }

        var (success, message) = await _smsService.VerifyCodeAsync(request.Phone, request.VerifyCode);
        
        if (success)
        {
            return ApiResponse<bool>.Ok(true, message);
        }
        
        return ApiResponse<bool>.Fail(message);
    }

    /// <summary>
    /// 检查手机号是否已注册
    /// </summary>
    /// <param name="phone">手机号</param>
    /// <returns>检查结果，true表示已存在</returns>
    [HttpGet("check-phone")]
    public async Task<ApiResponse<bool>> CheckPhoneExists([FromQuery] string phone)
    {
        if (string.IsNullOrEmpty(phone))
        {
            return ApiResponse<bool>.Fail("请输入手机号");
        }

        var result = await _userService.CheckPhoneExistsAsync(phone);
        return result;
    }

    /// <summary>
    /// 检查手机号是否已注册为房东
    /// </summary>
    /// <param name="phone">手机号</param>
    /// <returns>检查结果，true表示已存在</returns>
    [HttpGet("check-host-phone")]
    public async Task<ApiResponse<bool>> CheckHostPhoneExists([FromQuery] string phone)
    {
        if (string.IsNullOrEmpty(phone))
        {
            return ApiResponse<bool>.Fail("请输入手机号");
        }

        var result = await _userService.CheckHostPhoneExistsAsync(phone);
        return result;
    }

    /// <summary>
    /// 退出登录
    /// </summary>
    /// <returns>退出结果</returns>
    [HttpPost("logout")]
    public async Task<ApiResponse<bool>> Logout()
    {
        var stopwatch = Stopwatch.StartNew();
        var ipAddress = GetClientIpAddress();
        var (browser, os) = ParseUserAgent();
        var requestUrl = Request.Path.Value;
        var requestMethod = Request.Method;

        var userIdClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var usernameClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        
        long? userId = null;
        if (long.TryParse(userIdClaim, out var parsedId))
        {
            userId = parsedId;
        }

        stopwatch.Stop();

        await _logService.LogAsync(new OperationLogRequest
        {
            Type = "logout",
            OperatorId = userId,
            OperatorName = usernameClaim,
            Description = $"用户 [{usernameClaim}] 退出登录",
            IpAddress = ipAddress,
            Browser = browser,
            Os = os,
            RequestUrl = requestUrl,
            RequestMethod = requestMethod,
            ResponseCode = 200,
            Status = "success",
            Duration = (int)stopwatch.ElapsedMilliseconds
        });
        
        return ApiResponse<bool>.Ok(true, "退出成功");
    }

    /// <summary>
    /// 测试密码哈希（仅用于开发测试）
    /// </summary>
    /// <param name="request">测试请求</param>
    /// <returns>哈希结果</returns>
    [HttpPost("test-hash")]
    public ApiResponse<object> TestHash([FromBody] TestHashRequest request)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        bool? verify = null;
        if (!string.IsNullOrEmpty(request.StoredHash))
        {
            verify = BCrypt.Net.BCrypt.Verify(request.Password, request.StoredHash);
        }
        return ApiResponse<object>.Ok(new { 
            newPasswordHash = hash, 
            verifyResult = verify,
            inputPassword = request.Password,
            storedHash = request.StoredHash
        });
    }

    private string GetClientIpAddress()
    {
        string? ip = null;

        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            ip = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim())
                            .FirstOrDefault(s => !string.IsNullOrEmpty(s) && !s.Equals("unknown", StringComparison.OrdinalIgnoreCase));
        }

        if (string.IsNullOrEmpty(ip))
        {
            ip = Request.Headers["X-Real-IP"].FirstOrDefault();
        }

        if (string.IsNullOrEmpty(ip))
        {
            ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        return IpLocationService.NormalizeIpAddress(ip);
    }

    private (string? browser, string? os) ParseUserAgent()
    {
        var userAgent = Request.Headers.UserAgent.ToString();
        if (string.IsNullOrEmpty(userAgent))
        {
            return (null, null);
        }

        string? browser = null;
        string? os = null;

        if (userAgent.Contains("Chrome") && !userAgent.Contains("Edg"))
            browser = "Chrome";
        else if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome"))
            browser = "Safari";
        else if (userAgent.Contains("Firefox"))
            browser = "Firefox";
        else if (userAgent.Contains("Edg"))
            browser = "Edge";
        else if (userAgent.Contains("MSIE") || userAgent.Contains("Trident"))
            browser = "IE";

        if (userAgent.Contains("Windows NT 10"))
            os = "Windows 10";
        else if (userAgent.Contains("Windows NT 6.3"))
            os = "Windows 8.1";
        else if (userAgent.Contains("Windows NT 6.2"))
            os = "Windows 8";
        else if (userAgent.Contains("Windows NT 6.1"))
            os = "Windows 7";
        else if (userAgent.Contains("Mac OS X"))
            os = "macOS";
        else if (userAgent.Contains("Android"))
            os = "Android";
        else if (userAgent.Contains("iPhone") || userAgent.Contains("iPad"))
            os = "iOS";
        else if (userAgent.Contains("Linux"))
            os = "Linux";

        return (browser, os);
    }
}

/// <summary>
/// 测试哈希请求
/// </summary>
public class TestHashRequest
{
    /// <summary>
    /// 待哈希的密码
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// 已存储的哈希值（用于验证）
    /// </summary>
    public string StoredHash { get; set; } = string.Empty;
}
