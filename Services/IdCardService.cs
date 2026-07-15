using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using qisu_server.Models.DTOs;

namespace qisu_server.Services;

public interface IIdCardService
{
    Task<ApiResponse<IdCardVerifyResult>> VerifyAsync(string realName, string idCard);
}

public class IdCardService : IIdCardService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IdCardService> _logger;
    private readonly string _juheKey;

    public IdCardService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<IdCardService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _juheKey = configuration["Juhe:IdCardKey"] ?? "";
    }

    public async Task<ApiResponse<IdCardVerifyResult>> VerifyAsync(string realName, string idCard)
    {
        if (string.IsNullOrEmpty(_juheKey))
        {
            _logger.LogWarning("聚合数据身份证验证Key未配置");
            return ApiResponse<IdCardVerifyResult>.Fail("身份证验证服务未配置");
        }

        if (string.IsNullOrEmpty(realName) || string.IsNullOrEmpty(idCard))
        {
            return ApiResponse<IdCardVerifyResult>.Fail("姓名和身份证号不能为空");
        }

        if (idCard.Length != 15 && idCard.Length != 18)
        {
            return ApiResponse<IdCardVerifyResult>.Fail("身份证号格式不正确");
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"http://op.juhe.cn/idcard/query?key={_juheKey}&idcard={idCard}&realname={Uri.EscapeDataString(realName)}";
            
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("身份证验证响应: {Content}", content);

            var result = JsonSerializer.Deserialize<JuheIdCardResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
            {
                return ApiResponse<IdCardVerifyResult>.Fail("身份证验证服务响应异常");
            }

            if (result.ErrorCode == 0)
            {
                var verifyResult = new IdCardVerifyResult
                {
                    RealName = realName,
                    IdCard = MaskIdCard(idCard),
                    IsMatch = result.Result?.Res == 1,
                    Message = result.Result?.Res == 1 ? "身份信息一致" : "身份信息不一致"
                };
                return ApiResponse<IdCardVerifyResult>.Ok(verifyResult);
            }
            else
            {
                var errorMsg = result.ErrorCode switch
                {
                    10001 => "Key错误或不存在",
                    10002 => "服务已关闭",
                    10003 => "请求次数超限",
                    10004 => "IP请求超限",
                    10005 => "请求频率超限",
                    10006 => "参数错误",
                    _ => $"验证失败: {result.Reason}"
                };
                return ApiResponse<IdCardVerifyResult>.Fail(errorMsg);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "身份证验证异常");
            return ApiResponse<IdCardVerifyResult>.Fail("身份证验证服务异常，请稍后重试");
        }
    }

    private static string MaskIdCard(string idCard)
    {
        if (string.IsNullOrEmpty(idCard) || idCard.Length < 8)
            return idCard;
        
        return idCard.Substring(0, 4) + "**********" + idCard.Substring(idCard.Length - 4);
    }
}

public class IdCardVerifyResult
{
    public string RealName { get; set; } = string.Empty;
    public string IdCard { get; set; } = string.Empty;
    public bool IsMatch { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class JuheIdCardResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("error_code")]
    public int ErrorCode { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("resultcode")]
    public string ResultCode { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("result")]
    public JuheIdCardResult? Result { get; set; }
}

public class JuheIdCardResult
{
    [System.Text.Json.Serialization.JsonPropertyName("realname")]
    public string RealName { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("idcard")]
    public string IdCard { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("res")]
    public int Res { get; set; }
}
