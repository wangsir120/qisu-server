using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models;
using qisu_server.Models.DTOs;

namespace qisu_server.Controllers;

/// <summary>
/// 管理员个人中心控制器
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(AppDbContext context, ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取管理员个人信息
    /// </summary>
    /// <returns>管理员详细信息，包括用户名、姓名、头像、邮箱、手机号、角色等</returns>
    [HttpGet("profile")]
    public async Task<ApiResponse<AdminProfileDto>> GetProfile()
    {
        var adminId = GetCurrentUserId();
        if (adminId == null)
        {
            return ApiResponse<AdminProfileDto>.Fail("用户未登录");
        }

        var admin = await _context.Admins.FindAsync(adminId.Value);
        if (admin == null)
        {
            return ApiResponse<AdminProfileDto>.Fail("管理员不存在");
        }

        return ApiResponse<AdminProfileDto>.Ok(new AdminProfileDto
        {
            Id = admin.Id,
            Username = admin.Username,
            Name = admin.Name,
            Avatar = admin.Avatar,
            Email = admin.Email,
            Phone = admin.Phone,
            Role = admin.Role,
            LastLoginAt = admin.LastLoginAt,
            CreatedAt = admin.CreatedAt
        });
    }

    /// <summary>
    /// 更新管理员个人信息
    /// </summary>
    /// <param name="request">更新请求，包含姓名、邮箱、手机号</param>
    /// <returns>更新后的管理员信息</returns>
    [HttpPut("profile")]
    public async Task<ApiResponse<AdminProfileDto>> UpdateProfile([FromBody] UpdateAdminProfileRequest request)
    {
        var adminId = GetCurrentUserId();
        if (adminId == null)
        {
            return ApiResponse<AdminProfileDto>.Fail("用户未登录");
        }

        var admin = await _context.Admins.FindAsync(adminId.Value);
        if (admin == null)
        {
            return ApiResponse<AdminProfileDto>.Fail("管理员不存在");
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            admin.Name = request.Name;
        }

        if (!string.IsNullOrEmpty(request.Email))
        {
            admin.Email = request.Email;
        }

        if (!string.IsNullOrEmpty(request.Phone))
        {
            admin.Phone = request.Phone;
        }

        admin.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        return ApiResponse<AdminProfileDto>.Ok(new AdminProfileDto
        {
            Id = admin.Id,
            Username = admin.Username,
            Name = admin.Name,
            Avatar = admin.Avatar,
            Email = admin.Email,
            Phone = admin.Phone,
            Role = admin.Role,
            LastLoginAt = admin.LastLoginAt,
            CreatedAt = admin.CreatedAt
        }, "更新成功");
    }

    /// <summary>
    /// 更新管理员头像
    /// </summary>
    /// <param name="request">头像更新请求，包含头像URL</param>
    /// <returns>更新后的管理员信息</returns>
    [HttpPut("avatar")]
    public async Task<ApiResponse<AdminProfileDto>> UpdateAvatar([FromBody] UpdateAvatarRequest request)
    {
        var adminId = GetCurrentUserId();
        if (adminId == null)
        {
            return ApiResponse<AdminProfileDto>.Fail("用户未登录");
        }

        var admin = await _context.Admins.FindAsync(adminId.Value);
        if (admin == null)
        {
            return ApiResponse<AdminProfileDto>.Fail("管理员不存在");
        }

        admin.Avatar = request.Avatar;
        admin.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        _logger.LogInformation("管理员 {AdminId} 更新头像成功", adminId);

        return ApiResponse<AdminProfileDto>.Ok(new AdminProfileDto
        {
            Id = admin.Id,
            Username = admin.Username,
            Name = admin.Name,
            Avatar = admin.Avatar,
            Email = admin.Email,
            Phone = admin.Phone,
            Role = admin.Role,
            LastLoginAt = admin.LastLoginAt,
            CreatedAt = admin.CreatedAt
        }, "头像更新成功");
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

public class AdminProfileDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Avatar { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Role { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateAdminProfileRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public class UpdateAvatarRequest
{
    public string? Avatar { get; set; }
}
