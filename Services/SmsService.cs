using AlibabaCloud.SDK.Dypnsapi20170525.Models;
using AlibabaCloud.TeaUtil.Models;

namespace qisu_server.Services;

public interface ISmsService
{
    Task<(bool Success, string Message, string? VerifyCode)> SendVerifyCodeAsync(string phoneNumber);
    Task<(bool Success, string Message)> VerifyCodeAsync(string phoneNumber, string code);
    bool ValidateCodeLocally(string phoneNumber, string code);
}

public class SmsService : ISmsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmsService> _logger;
    private readonly AlibabaCloud.SDK.Dypnsapi20170525.Client _client;
    
    private static readonly Dictionary<string, (string code, DateTime expireTime, DateTime sendTime)> _smsCodeStore = new();
    private static readonly object _lock = new();
    private const int CodeLength = 6;
    private const int ExpireMinutes = 5;
    private const int SendIntervalSeconds = 60;

    private readonly string _signName;
    private readonly string _templateCode;

    public SmsService(IConfiguration configuration, ILogger<SmsService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        _signName = configuration["Aliyun:Sms:SignName"] ?? "";
        _templateCode = configuration["Aliyun:Sms:TemplateCode"] ?? "";
        
        _client = CreateClient();
    }

    private AlibabaCloud.SDK.Dypnsapi20170525.Client CreateClient()
    {
        AlibabaCloud.OpenApiClient.Models.Config config;
        
        var accessKeyId = _configuration["Aliyun:AccessKeyId"];
        var accessKeySecret = _configuration["Aliyun:AccessKeySecret"];
        
        if (!string.IsNullOrEmpty(accessKeyId) && !string.IsNullOrEmpty(accessKeySecret))
        {
            config = new AlibabaCloud.OpenApiClient.Models.Config
            {
                AccessKeyId = accessKeyId,
                AccessKeySecret = accessKeySecret,
            };
        }
        else
        {
            Aliyun.Credentials.Client credential = new Aliyun.Credentials.Client();
            config = new AlibabaCloud.OpenApiClient.Models.Config
            {
                Credential = credential,
            };
        }
        
        config.Endpoint = "dypnsapi.aliyuncs.com";
        return new AlibabaCloud.SDK.Dypnsapi20170525.Client(config);
    }

    public async Task<(bool Success, string Message, string? VerifyCode)> SendVerifyCodeAsync(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber))
        {
            return (false, "手机号不能为空", null);
        }

        if (!IsValidPhoneNumber(phoneNumber))
        {
            return (false, "手机号格式不正确", null);
        }

        lock (_lock)
        {
            if (_smsCodeStore.TryGetValue(phoneNumber, out var stored))
            {
                var timeSinceLastSend = DateTime.Now - stored.sendTime;
                if (timeSinceLastSend.TotalSeconds < SendIntervalSeconds)
                {
                    var remainingSeconds = SendIntervalSeconds - (int)timeSinceLastSend.TotalSeconds;
                    return (false, $"请{remainingSeconds}秒后再试", null);
                }
            }
        }
        
        try
        {
            var request = new SendSmsVerifyCodeRequest
            {
                PhoneNumber = phoneNumber,
                SignName = _signName,
                TemplateCode = _templateCode,
                TemplateParam = $"{{\"code\":\"##code##\",\"min\":\"{ExpireMinutes}\"}}",
                CodeLength = CodeLength,
                ValidTime = ExpireMinutes * 60,
                CodeType = 1,
                ReturnVerifyCode = true,
            };
            
            var runtime = new RuntimeOptions();
            var response = await _client.SendSmsVerifyCodeWithOptionsAsync(request, runtime);
            
            if (response.Body.Code == "OK")
            {
                var verifyCode = response.Body.Model?.VerifyCode ?? "";
                
                lock (_lock)
                {
                    CleanExpiredCodes();
                    _smsCodeStore[phoneNumber] = (verifyCode, DateTime.Now.AddMinutes(ExpireMinutes), DateTime.Now);
                }
                
                _logger.LogInformation("验证码发送成功，手机号：{PhoneNumber}，验证码：{Code}", phoneNumber, verifyCode);
                return (true, "验证码发送成功", verifyCode);
            }
            else
            {
                _logger.LogWarning("验证码发送失败，手机号：{PhoneNumber}，错误码：{Code}，错误信息：{Message}", 
                    phoneNumber, response.Body.Code, response.Body.Message);
                return (false, $"发送失败：{response.Body.Message}", null);
            }
        }
        catch (Tea.TeaException error)
        {
            _logger.LogError(error, "验证码发送异常，手机号：{PhoneNumber}，错误：{Message}", phoneNumber, error.Message);
            return (false, $"发送失败：{error.Message}", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证码发送异常，手机号：{PhoneNumber}，内部异常：{InnerException}", 
                phoneNumber, ex.InnerException?.Message ?? ex.Message);
            return (false, $"发送失败：{ex.InnerException?.Message ?? ex.Message}", null);
        }
    }

    public async Task<(bool Success, string Message)> VerifyCodeAsync(string phoneNumber, string code)
    {
        if (string.IsNullOrEmpty(phoneNumber))
        {
            return (false, "手机号不能为空");
        }

        if (string.IsNullOrEmpty(code))
        {
            return (false, "验证码不能为空");
        }

        try
        {
            var request = new CheckSmsVerifyCodeRequest
            {
                PhoneNumber = phoneNumber,
                VerifyCode = code,
            };
            
            var runtime = new RuntimeOptions();
            var response = await _client.CheckSmsVerifyCodeWithOptionsAsync(request, runtime);
            
            _logger.LogInformation("验证码验证响应，手机号：{PhoneNumber}，Code：{Code}，VerifyResult：{VerifyResult}，Message：{Message}", 
                phoneNumber, response.Body.Code, response.Body.Model?.VerifyResult, response.Body.Message);
            
            if (response.Body.Code == "OK" && response.Body.Model?.VerifyResult == "PASS")
            {
                lock (_lock)
                {
                    _smsCodeStore.Remove(phoneNumber);
                }
                
                _logger.LogInformation("验证码验证成功，手机号：{PhoneNumber}", phoneNumber);
                return (true, "验证成功");
            }
            else
            {
                var errorMsg = response.Body.Model?.VerifyResult == "UNKNOWN" 
                    ? "验证码错误或已过期，请重新获取" 
                    : $"验证失败：{response.Body.Message}";
                
                _logger.LogWarning("验证码验证失败，手机号：{PhoneNumber}，错误码：{Code}，VerifyResult：{VerifyResult}，错误信息：{Message}", 
                    phoneNumber, response.Body.Code, response.Body.Model?.VerifyResult, response.Body.Message);
                return (false, errorMsg);
            }
        }
        catch (Tea.TeaException error)
        {
            _logger.LogError(error, "验证码验证异常，手机号：{PhoneNumber}，错误码：{ErrorCode}，错误：{Message}", 
                phoneNumber, error.Code, error.Message);
            
            if (error.Code == "400")
            {
                return (false, "验证码已过期或无效，请重新获取验证码");
            }
            
            return (false, $"验证失败：{error.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证码验证异常，手机号：{PhoneNumber}", phoneNumber);
            return (false, "验证失败，请稍后重试");
        }
    }

    public bool ValidateCodeLocally(string phoneNumber, string code)
    {
        if (string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(code))
            return false;

        lock (_lock)
        {
            if (!_smsCodeStore.TryGetValue(phoneNumber, out var stored))
                return false;

            if (DateTime.Now > stored.expireTime)
            {
                _smsCodeStore.Remove(phoneNumber);
                return false;
            }

            if (stored.code != code)
                return false;

            _smsCodeStore.Remove(phoneNumber);
            return true;
        }
    }

    private static bool IsValidPhoneNumber(string phone)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(phone, @"^1[3-9]\d{9}$");
    }

    private static void CleanExpiredCodes()
    {
        var expiredKeys = _smsCodeStore
            .Where(x => DateTime.Now > x.Value.expireTime)
            .Select(x => x.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _smsCodeStore.Remove(key);
        }
    }
}
