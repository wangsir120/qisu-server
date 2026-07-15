using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models.DTOs;

namespace qisu_server.Controllers;

/// <summary>
/// 系统通知控制器
/// </summary>
[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(AppDbContext context, ILogger<NotificationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取通知列表
    /// </summary>
    /// <param name="page">页码，默认1</param>
    /// <param name="pageSize">每页数量，默认10</param>
    /// <returns>通知列表，根据用户角色筛选（管理员看admin通知，房东看landlord通知）</returns>
    [HttpGet]
    public async Task<ApiResponse<List<NotificationDto>>> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return ApiResponse<List<NotificationDto>>.Fail("用户未登录");
            }

            var isAdmin = await IsAdmin(userId.Value);
            
            var query = _context.SystemNotifications
                .Where(n => n.TargetUserId == null || n.TargetUserId == userId.Value);
            
            if (!isAdmin)
            {
                query = query.Where(n => n.TargetRole == null || n.TargetRole == "landlord");
            }
            else
            {
                query = query.Where(n => n.TargetRole == null || n.TargetRole == "admin");
            }

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Content = n.Content,
                Type = n.Type,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToList();

            return ApiResponse<List<NotificationDto>>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取通知失败");
            return ApiResponse<List<NotificationDto>>.Fail("获取通知失败");
        }
    }

    /// <summary>
    /// 获取未读通知数量
    /// </summary>
    /// <returns>未读通知总数</returns>
    [HttpGet("unread-count")]
    public async Task<ApiResponse<int>> GetUnreadCount()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return ApiResponse<int>.Fail("用户未登录");
            }

            var isAdmin = await IsAdmin(userId.Value);
            
            var query = _context.SystemNotifications
                .Where(n => !n.IsRead && (n.TargetUserId == null || n.TargetUserId == userId.Value));
            
            if (!isAdmin)
            {
                query = query.Where(n => n.TargetRole == null || n.TargetRole == "landlord");
            }
            else
            {
                query = query.Where(n => n.TargetRole == null || n.TargetRole == "admin");
            }

            var count = await query.CountAsync();

            return ApiResponse<int>.Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取未读数失败");
            return ApiResponse<int>.Fail("获取未读数失败");
        }
    }

    /// <summary>
    /// 标记单条通知为已读
    /// </summary>
    /// <param name="id">通知ID</param>
    /// <returns>操作结果</returns>
    [HttpPut("{id}/read")]
    public async Task<ApiResponse<bool>> MarkAsRead(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return ApiResponse<bool>.Fail("用户未登录");
            }

            var notification = await _context.SystemNotifications.FindAsync(id);
            if (notification == null)
            {
                return ApiResponse<bool>.Fail("通知不存在");
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "标记已读失败");
            return ApiResponse<bool>.Fail("标记已读失败");
        }
    }

    /// <summary>
    /// 标记所有通知为已读
    /// </summary>
    /// <returns>操作结果</returns>
    [HttpPut("read-all")]
    public async Task<ApiResponse<bool>> MarkAllAsRead()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return ApiResponse<bool>.Fail("用户未登录");
            }

            var isAdmin = await IsAdmin(userId.Value);
            
            var query = _context.SystemNotifications
                .Where(n => !n.IsRead && (n.TargetUserId == null || n.TargetUserId == userId.Value));
            
            if (!isAdmin)
            {
                query = query.Where(n => n.TargetRole == null || n.TargetRole == "landlord");
            }
            else
            {
                query = query.Where(n => n.TargetRole == null || n.TargetRole == "admin");
            }

            var notifications = await query.ToListAsync();
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "全部标记已读失败");
            return ApiResponse<bool>.Fail("全部标记已读失败");
        }
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

    private async Task<bool> IsAdmin(long userId)
    {
        var admin = await _context.Admins.FindAsync(userId);
        return admin != null;
    }
}
