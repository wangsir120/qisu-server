using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models;
using qisu_server.Models.DTOs;

namespace qisu_server.Controllers;

/// <summary>
/// 公开公告接口控制器（无需登录即可访问）
/// </summary>
[ApiController]
[Route("api/announcements")]
public class PublicAnnouncementsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PublicAnnouncementsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取已发布公告列表
    /// </summary>
    /// <param name="limit">返回数量限制，默认10条</param>
    /// <param name="type">类型筛选：notice-通知，activity-活动</param>
    /// <returns>公告列表，包含已读状态（需登录）</returns>
    [HttpGet]
    public async Task<ApiResponse<List<AnnouncementDto>>> GetList(
        [FromQuery] int limit = 10,
        [FromQuery] string? type = null)
    {
        var now = DateTime.Now;
        var userId = GetCurrentUserId();
        
        var query = _context.Announcements
            .Where(a => a.Status == "published")
            .Where(a => !a.StartTime.HasValue || a.StartTime <= now)
            .Where(a => !a.EndTime.HasValue || a.EndTime >= now);

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(a => a.Type == type);
        }

        var announcements = await query
            .OrderByDescending(a => a.IsTop)
            .ThenByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync();

        var announcementIds = announcements.Select(a => a.Id).ToList();
        
        var readAnnouncementIds = new HashSet<long>();
        if (userId.HasValue)
        {
            var readList = await _context.UserAnnouncementReads
                .Where(r => r.UserId == userId.Value && announcementIds.Contains(r.AnnouncementId))
                .Select(r => r.AnnouncementId)
                .ToListAsync();
            readAnnouncementIds = new HashSet<long>(readList);
        }

        var items = announcements.Select(a => new AnnouncementDto
        {
            Id = a.Id,
            Title = a.Title,
            Content = a.Content,
            Type = a.Type,
            IsTop = a.IsTop,
            ViewCount = a.ViewCount,
            CreatedAt = a.CreatedAt,
            IsRead = readAnnouncementIds.Contains(a.Id)
        }).ToList();

        return ApiResponse<List<AnnouncementDto>>.Ok(items);
    }

    /// <summary>
    /// 获取公告详情
    /// </summary>
    /// <param name="id">公告ID</param>
    /// <returns>公告详情，访问后自动增加浏览量</returns>
    [HttpGet("{id}")]
    public async Task<ApiResponse<AnnouncementDto>> GetById(long id)
    {
        var announcement = await _context.Announcements.FindAsync(id);
        if (announcement == null || announcement.Status != "published")
        {
            return ApiResponse<AnnouncementDto>.Fail("公告不存在");
        }

        announcement.ViewCount++;
        await _context.SaveChangesAsync();

        var userId = GetCurrentUserId();
        var isRead = false;
        if (userId.HasValue)
        {
            isRead = await _context.UserAnnouncementReads
                .AnyAsync(r => r.UserId == userId.Value && r.AnnouncementId == id);
        }

        var dto = new AnnouncementDto
        {
            Id = announcement.Id,
            Title = announcement.Title,
            Content = announcement.Content,
            Type = announcement.Type,
            IsTop = announcement.IsTop,
            ViewCount = announcement.ViewCount,
            StartTime = announcement.StartTime,
            EndTime = announcement.EndTime,
            CreatedAt = announcement.CreatedAt,
            IsRead = isRead
        };

        return ApiResponse<AnnouncementDto>.Ok(dto);
    }

    /// <summary>
    /// 标记公告为已读
    /// </summary>
    /// <param name="id">公告ID</param>
    /// <returns>操作结果</returns>
    /// <remarks>需要登录</remarks>
    [HttpPost("{id}/read")]
    [Authorize]
    public async Task<ApiResponse<bool>> MarkAsRead(long id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<bool>.Fail("用户未登录");
        }

        var announcement = await _context.Announcements.FindAsync(id);
        if (announcement == null || announcement.Status != "published")
        {
            return ApiResponse<bool>.Fail("公告不存在");
        }

        var existingRead = await _context.UserAnnouncementReads
            .FirstOrDefaultAsync(r => r.UserId == userId.Value && r.AnnouncementId == id);

        if (existingRead != null)
        {
            return ApiResponse<bool>.Ok(true, "已标记为已读");
        }

        var userAnnouncementRead = new UserAnnouncementRead
        {
            UserId = userId.Value,
            AnnouncementId = id,
            ReadAt = DateTime.Now
        };

        _context.UserAnnouncementReads.Add(userAnnouncementRead);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "已标记为已读");
    }

    /// <summary>
    /// 标记所有公告为已读
    /// </summary>
    /// <returns>操作结果，包含标记数量</returns>
    /// <remarks>需要登录</remarks>
    [HttpPost("read-all")]
    [Authorize]
    public async Task<ApiResponse<bool>> MarkAllAsRead()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<bool>.Fail("用户未登录");
        }

        var now = DateTime.Now;
        
        var publishedAnnouncementIds = await _context.Announcements
            .Where(a => a.Status == "published")
            .Where(a => !a.StartTime.HasValue || a.StartTime <= now)
            .Where(a => !a.EndTime.HasValue || a.EndTime >= now)
            .Select(a => a.Id)
            .ToListAsync();

        var alreadyReadIds = await _context.UserAnnouncementReads
            .Where(r => r.UserId == userId.Value)
            .Select(r => r.AnnouncementId)
            .ToListAsync();

        var unreadIds = publishedAnnouncementIds.Except(alreadyReadIds).ToList();

        foreach (var announcementId in unreadIds)
        {
            _context.UserAnnouncementReads.Add(new UserAnnouncementRead
            {
                UserId = userId.Value,
                AnnouncementId = announcementId,
                ReadAt = DateTime.Now
            });
        }

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, $"已将 {unreadIds.Count} 条公告标记为已读");
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
            return null;
        }
        return long.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
