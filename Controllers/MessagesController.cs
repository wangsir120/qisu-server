using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models;
using qisu_server.Models.DTOs;

namespace qisu_server.Controllers;

/// <summary>
/// 消息管理控制器
/// </summary>
[ApiController]
[Route("api/messages")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(AppDbContext context, ILogger<MessagesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取未读消息数量
    /// </summary>
    /// <returns>未读消息数量，包含系统消息和未读公告</returns>
    [HttpGet("unread-count")]
    public async Task<ApiResponse<int>> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<int>.Fail("用户未登录");
        }

        var messageCount = await _context.Messages
            .CountAsync(m => m.UserId == userId && !m.IsRead);

        var now = DateTime.Now;
        var publishedAnnouncementIds = await _context.Announcements
            .Where(a => a.Status == "published")
            .Where(a => !a.StartTime.HasValue || a.StartTime <= now)
            .Where(a => !a.EndTime.HasValue || a.EndTime >= now)
            .Select(a => a.Id)
            .ToListAsync();

        var readAnnouncementIds = await _context.UserAnnouncementReads
            .Where(r => r.UserId == userId)
            .Select(r => r.AnnouncementId)
            .ToListAsync();

        var unreadAnnouncementCount = publishedAnnouncementIds.Except(readAnnouncementIds).Count();

        return ApiResponse<int>.Ok(messageCount + unreadAnnouncementCount);
    }

    /// <summary>
    /// 获取消息列表
    /// </summary>
    /// <param name="page">页码，默认1</param>
    /// <param name="pageSize">每页数量，默认10</param>
    /// <returns>分页的消息列表</returns>
    [HttpGet("list")]
    public async Task<ApiResponse<PagedResult<MessageDto>>> GetList([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<PagedResult<MessageDto>>.Fail("用户未登录");
        }

        var query = _context.Messages
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt);

        var total = await query.CountAsync();

        var messages = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                Title = m.Title,
                Content = m.Content,
                Type = m.Type,
                IsRead = m.IsRead,
                RelatedId = m.RelatedId,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();

        var result = new PagedResult<MessageDto>
        {
            Items = messages,
            Total = total,
            Page = page,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<MessageDto>>.Ok(result);
    }

    /// <summary>
    /// 标记消息为已读
    /// </summary>
    /// <param name="id">消息ID</param>
    /// <returns>操作结果</returns>
    [HttpPost("{id}/read")]
    public async Task<ApiResponse<bool>> MarkAsRead(long id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<bool>.Fail("用户未登录");
        }

        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

        if (message == null)
        {
            return ApiResponse<bool>.Fail("消息不存在");
        }

        message.IsRead = true;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "已标记为已读");
    }

    /// <summary>
    /// 标记所有消息为已读
    /// </summary>
    /// <returns>操作结果</returns>
    [HttpPost("read-all")]
    public async Task<ApiResponse<bool>> MarkAllAsRead()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<bool>.Fail("用户未登录");
        }

        var unreadMessages = await _context.Messages
            .Where(m => m.UserId == userId && !m.IsRead)
            .ToListAsync();

        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
        }

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, $"已将 {unreadMessages.Count} 条消息标记为已读");
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

/// <summary>
/// 消息数据传输对象
/// </summary>
public class MessageDto
{
    public long Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Type { get; set; }
    public bool IsRead { get; set; }
    public long? RelatedId { get; set; }
    public DateTime CreatedAt { get; set; }
}
