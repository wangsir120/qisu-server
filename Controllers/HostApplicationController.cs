using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using qisu_server.Models.DTOs;
using qisu_server.Services;

namespace qisu_server.Controllers;

/// <summary>
/// 房东申请管理控制器
/// </summary>
[ApiController]
[Route("api/host-application")]
[Authorize]
public class HostApplicationController : ControllerBase
{
    private readonly IHostApplicationService _hostApplicationService;
    private readonly ILogger<HostApplicationController> _logger;

    public HostApplicationController(IHostApplicationService hostApplicationService, ILogger<HostApplicationController> logger)
    {
        _hostApplicationService = hostApplicationService;
        _logger = logger;
    }

    /// <summary>
    /// 获取房东申请统计数据
    /// </summary>
    /// <returns>统计数据，包含待审核、已通过、已拒绝数量</returns>
    [HttpGet("stats")]
    public async Task<ApiResponse<HostApplicationStatsDto>> GetStats()
    {
        return await _hostApplicationService.GetStatsAsync();
    }

    /// <summary>
    /// 获取房东申请列表
    /// </summary>
    /// <param name="request">查询参数，支持状态筛选和关键词搜索</param>
    /// <returns>分页的申请列表</returns>
    [HttpGet("list")]
    public async Task<ApiResponse<PagedResult<HostApplicationListDto>>> GetList([FromQuery] HostApplicationQueryRequest request)
    {
        return await _hostApplicationService.GetListAsync(request);
    }

    /// <summary>
    /// 获取房东申请详情
    /// </summary>
    /// <param name="id">申请ID</param>
    /// <returns>申请详情信息</returns>
    [HttpGet("{id}")]
    public async Task<ApiResponse<HostApplicationDetailDto>> GetById(long id)
    {
        return await _hostApplicationService.GetByIdAsync(id);
    }

    /// <summary>
    /// 审核房东申请
    /// </summary>
    /// <param name="id">申请ID</param>
    /// <param name="request">审核请求，包含审核状态和备注</param>
    /// <returns>审核结果</returns>
    /// <remarks>
    /// 审核通过后会自动创建房东记录到hosts表，并发送系统消息通知申请人
    /// </remarks>
    [HttpPost("{id}/audit")]
    public async Task<ApiResponse<bool>> Audit(long id, [FromBody] HostApplicationAuditRequest request)
    {
        var auditorIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(auditorIdStr) || !long.TryParse(auditorIdStr, out var auditorId))
        {
            return ApiResponse<bool>.Fail("无法获取审核人信息");
        }
        var auditorName = User.FindFirst(ClaimTypes.Name)?.Value;
        return await _hostApplicationService.AuditAsync(id, request, auditorId, auditorName);
    }
}
