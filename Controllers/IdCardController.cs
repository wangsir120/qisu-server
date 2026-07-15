using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using qisu_server.Models.DTOs;
using qisu_server.Services;

namespace qisu_server.Controllers;

/// <summary>
/// 身份证验证控制器
/// </summary>
[ApiController]
[Route("api/idcard")]
public class IdCardController : ControllerBase
{
    private readonly IIdCardService _idCardService;
    private readonly ILogger<IdCardController> _logger;

    public IdCardController(IIdCardService idCardService, ILogger<IdCardController> logger)
    {
        _idCardService = idCardService;
        _logger = logger;
    }

    /// <summary>
    /// 身份证实名认证验证
    /// </summary>
    /// <param name="request">验证请求，包含姓名和身份证号</param>
    /// <returns>验证结果</returns>
    /// <remarks>
    /// 调用聚合数据API进行身份证二要素验证，核验姓名和身份证号是否一致
    /// </remarks>
    [HttpPost("verify")]
    [Authorize]
    public async Task<ApiResponse<IdCardVerifyResult>> Verify([FromBody] IdCardVerifyRequest request)
    {
        return await _idCardService.VerifyAsync(request.RealName, request.IdCard);
    }
}
