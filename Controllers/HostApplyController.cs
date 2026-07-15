using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using qisu_server.Models.DTOs;
using qisu_server.Services;

namespace qisu_server.Controllers;

/// <summary>
/// C端房东申请控制器
/// </summary>
[ApiController]
[Route("api/host-apply")]
[Authorize]
public class HostApplyController : ControllerBase
{
    private readonly IHostApplyService _hostApplyService;
    private readonly ILogger<HostApplyController> _logger;

    public HostApplyController(IHostApplyService hostApplyService, ILogger<HostApplyController> logger)
    {
        _hostApplyService = hostApplyService;
        _logger = logger;
    }

    /// <summary>
    /// 提交房东申请
    /// </summary>
    /// <param name="request">申请信息</param>
    /// <returns>申请结果</returns>
    /// <remarks>
    /// 提交房东入驻申请，会先进行身份证实名认证验证
    /// </remarks>
    [HttpPost("submit")]
    public async Task<ApiResponse<bool>> Submit([FromBody] HostApplyRequest request)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var userId))
        {
            return ApiResponse<bool>.Fail("请先登录");
        }
        return await _hostApplyService.ApplyAsync(userId, request);
    }

    /// <summary>
    /// 获取房东申请状态
    /// </summary>
    /// <returns>申请状态信息</returns>
    /// <remarks>
    /// 获取当前用户的房东申请状态，包括是否已申请、审核状态等
    /// </remarks>
    [HttpGet("status")]
    public async Task<ApiResponse<HostApplyStatusDto>> GetStatus()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var userId))
        {
            return ApiResponse<HostApplyStatusDto>.Fail("请先登录");
        }
        return await _hostApplyService.GetStatusAsync(userId);
    }
}
