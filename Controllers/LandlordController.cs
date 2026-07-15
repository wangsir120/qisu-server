using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models.DTOs;
using qisu_server.Services;

namespace qisu_server.Controllers;

/// <summary>
/// 房东管理控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LandlordController : ControllerBase
{
    private readonly ILandlordService _landlordService;
    private readonly AppDbContext _context;
    private readonly ILogger<LandlordController> _logger;

    public LandlordController(ILandlordService landlordService, AppDbContext context, ILogger<LandlordController> logger)
    {
        _landlordService = landlordService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取房东统计数据
    /// </summary>
    /// <returns>统计数据，包含总数、活跃数、超赞房东数、本月新增数</returns>
    [HttpGet("stats")]
    public async Task<ApiResponse<LandlordStatsDto>> GetStats()
    {
        return await _landlordService.GetStatsAsync();
    }

    /// <summary>
    /// 获取房东列表
    /// </summary>
    /// <param name="request">查询参数，支持关键词搜索和分页</param>
    /// <returns>分页的房东列表</returns>
    [HttpGet("list")]
    public async Task<ApiResponse<PagedResult<LandlordListDto>>> GetList([FromQuery] LandlordQueryRequest request)
    {
        return await _landlordService.GetListAsync(request);
    }

    /// <summary>
    /// 获取房东详情
    /// </summary>
    /// <param name="id">房东ID</param>
    /// <returns>房东详情信息</returns>
    [HttpGet("{id}")]
    public async Task<ApiResponse<LandlordDetailDto>> GetById(long id)
    {
        return await _landlordService.GetByIdAsync(id);
    }

    /// <summary>
    /// 切换房东账号状态
    /// </summary>
    /// <param name="id">房东ID</param>
    /// <returns>操作结果</returns>
    /// <remarks>
    /// 启用/禁用房东账号
    /// </remarks>
    [HttpPost("{id}/toggle-status")]
    public async Task<ApiResponse<bool>> ToggleStatus(long id)
    {
        return await _landlordService.ToggleStatusAsync(id);
    }

    /// <summary>
    /// 更新房东头像
    /// </summary>
    /// <param name="request">头像更新请求</param>
    /// <returns>更新结果</returns>
    [HttpPut("avatar")]
    public async Task<ApiResponse<bool>> UpdateAvatar([FromBody] UpdateAvatarRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        }
        if (string.IsNullOrEmpty(userIdClaim))
        {
            userIdClaim = User.FindFirst("sub")?.Value;
        }
        
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
        {
            return ApiResponse<bool>.Fail("用户未登录");
        }

        var host = await _context.Hosts.FirstOrDefaultAsync(h => h.UserId == userId);
        if (host == null)
        {
            return ApiResponse<bool>.Fail("房东信息不存在");
        }

        host.Avatar = request.Avatar;
        host.UpdatedAt = DateTime.Now;
        
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.Avatar = request.Avatar;
            user.UpdatedAt = DateTime.Now;
        }
        
        await _context.SaveChangesAsync();

        _logger.LogInformation("房东 {UserId} 更新头像成功", userId);

        return ApiResponse<bool>.Ok(true, "头像更新成功");
    }

    /// <summary>
    /// 更新房东个人资料
    /// </summary>
    /// <param name="request">个人资料更新请求</param>
    /// <returns>更新结果</returns>
    [HttpPut("profile")]
    public async Task<ApiResponse<bool>> UpdateProfile([FromBody] UpdateLandlordProfileRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<bool>.Fail("用户未登录");
        }

        var host = await _context.Hosts.FirstOrDefaultAsync(h => h.UserId == userId.Value);
        if (host == null)
        {
            return ApiResponse<bool>.Fail("房东信息不存在");
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            host.Name = request.Name;
        }

        if (!string.IsNullOrEmpty(request.Phone))
        {
            host.Phone = request.Phone;
        }

        host.UpdatedAt = DateTime.Now;
        
        var user = await _context.Users.FindAsync(userId.Value);
        if (user != null)
        {
            if (!string.IsNullOrEmpty(request.Name))
            {
                user.Nickname = request.Name;
            }
            if (!string.IsNullOrEmpty(request.Phone))
            {
                user.Phone = request.Phone;
            }
            user.UpdatedAt = DateTime.Now;
        }
        
        await _context.SaveChangesAsync();

        _logger.LogInformation("房东 {UserId} 更新个人资料成功", userId);

        return ApiResponse<bool>.Ok(true, "个人资料更新成功");
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        }
        if (string.IsNullOrEmpty(userIdClaim))
        {
            userIdClaim = User.FindFirst("sub")?.Value;
        }
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return null;
        }
        return long.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

public class UpdateLandlordProfileRequest
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
}
