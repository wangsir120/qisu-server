using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models;
using qisu_server.Models.DTOs;

namespace qisu_server.Controllers;

/// <summary>
/// 公告管理控制器
/// </summary>
[ApiController]
[Route("api/admin/announcements")]
[Authorize]
public class AnnouncementsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AnnouncementsController> _logger;

    public AnnouncementsController(AppDbContext context, ILogger<AnnouncementsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取公告列表
    /// </summary>
    /// <param name="page">页码，默认1</param>
    /// <param name="pageSize">每页数量，默认10</param>
    /// <param name="status">状态筛选：draft-草稿，published-已发布</param>
    /// <param name="type">类型筛选：notice-通知，activity-活动</param>
    /// <param name="keyword">关键词搜索，匹配标题和内容</param>
    /// <returns>分页的公告列表</returns>
    [HttpGet]
    public async Task<ApiResponse<PagedResult<AnnouncementDto>>> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? type = null,
        [FromQuery] string? keyword = null)
    {
        var query = _context.Announcements.AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(a => a.Status == status);
        }

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(a => a.Type == type);
        }

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(a => a.Title.Contains(keyword) || a.Content.Contains(keyword));
        }

        query = query.OrderByDescending(a => a.IsTop).ThenByDescending(a => a.CreatedAt);

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AnnouncementDto
            {
                Id = a.Id,
                Title = a.Title,
                Content = a.Content,
                Type = a.Type,
                Status = a.Status,
                IsTop = a.IsTop,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                ViewCount = a.ViewCount,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync();

        var result = new PagedResult<AnnouncementDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<AnnouncementDto>>.Ok(result);
    }

    /// <summary>
    /// 获取公告详情
    /// </summary>
    /// <param name="id">公告ID</param>
    /// <returns>公告详情信息</returns>
    [HttpGet("{id}")]
    public async Task<ApiResponse<AnnouncementDto>> GetById(long id)
    {
        var announcement = await _context.Announcements.FindAsync(id);
        if (announcement == null)
        {
            return ApiResponse<AnnouncementDto>.Fail("公告不存在");
        }

        var dto = new AnnouncementDto
        {
            Id = announcement.Id,
            Title = announcement.Title,
            Content = announcement.Content,
            Type = announcement.Type,
            Status = announcement.Status,
            IsTop = announcement.IsTop,
            StartTime = announcement.StartTime,
            EndTime = announcement.EndTime,
            ViewCount = announcement.ViewCount,
            CreatedAt = announcement.CreatedAt,
            UpdatedAt = announcement.UpdatedAt
        };

        return ApiResponse<AnnouncementDto>.Ok(dto);
    }

    /// <summary>
    /// 创建公告
    /// </summary>
    /// <param name="request">公告创建请求</param>
    /// <returns>创建的公告信息</returns>
    [HttpPost]
    public async Task<ApiResponse<AnnouncementDto>> Create([FromBody] CreateAnnouncementRequest request)
    {
        var announcement = new Announcement
        {
            Title = request.Title,
            Content = request.Content,
            Type = request.Type ?? "notice",
            Status = request.Status ?? "draft",
            IsTop = request.IsTop,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            CreatedAt = DateTime.Now
        };

        _context.Announcements.Add(announcement);
        await _context.SaveChangesAsync();

        var dto = new AnnouncementDto
        {
            Id = announcement.Id,
            Title = announcement.Title,
            Content = announcement.Content,
            Type = announcement.Type,
            Status = announcement.Status,
            IsTop = announcement.IsTop,
            StartTime = announcement.StartTime,
            EndTime = announcement.EndTime,
            ViewCount = announcement.ViewCount,
            CreatedAt = announcement.CreatedAt
        };

        return ApiResponse<AnnouncementDto>.Ok(dto, "创建成功");
    }

    /// <summary>
    /// 更新公告信息
    /// </summary>
    /// <param name="id">公告ID</param>
    /// <param name="request">公告更新请求</param>
    /// <returns>更新结果</returns>
    [HttpPut("{id}")]
    public async Task<ApiResponse<bool>> Update(long id, [FromBody] UpdateAnnouncementRequest request)
    {
        var announcement = await _context.Announcements.FindAsync(id);
        if (announcement == null)
        {
            return ApiResponse<bool>.Fail("公告不存在");
        }

        if (!string.IsNullOrEmpty(request.Title))
            announcement.Title = request.Title;
        if (request.Content != null)
            announcement.Content = request.Content;
        if (request.Type != null)
            announcement.Type = request.Type;
        if (request.Status != null)
            announcement.Status = request.Status;
        if (request.IsTop.HasValue)
            announcement.IsTop = request.IsTop.Value;
        if (request.StartTime.HasValue)
            announcement.StartTime = request.StartTime;
        if (request.EndTime.HasValue)
            announcement.EndTime = request.EndTime;

        announcement.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "更新成功");
    }

    /// <summary>
    /// 删除公告
    /// </summary>
    /// <param name="id">公告ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    public async Task<ApiResponse<bool>> Delete(long id)
    {
        var announcement = await _context.Announcements.FindAsync(id);
        if (announcement == null)
        {
            return ApiResponse<bool>.Fail("公告不存在");
        }

        _context.Announcements.Remove(announcement);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "删除成功");
    }

    /// <summary>
    /// 发布公告
    /// </summary>
    /// <param name="id">公告ID</param>
    /// <returns>发布结果</returns>
    /// <remarks>
    /// 发布后会通过SSE推送消息通知所有在线用户
    /// </remarks>
    [HttpPost("{id}/publish")]
    public async Task<ApiResponse<bool>> Publish(long id)
    {
        var announcement = await _context.Announcements.FindAsync(id);
        if (announcement == null)
        {
            return ApiResponse<bool>.Fail("公告不存在");
        }

        announcement.Status = "published";
        announcement.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        await SseController.NotifyAllAsync("message", new
        {
            title = announcement.Title,
            content = announcement.Content,
            type = "announcement"
        });

        return ApiResponse<bool>.Ok(true, "发布成功");
    }

    /// <summary>
    /// 切换公告置顶状态
    /// </summary>
    /// <param name="id">公告ID</param>
    /// <returns>操作结果</returns>
    [HttpPost("{id}/top")]
    public async Task<ApiResponse<bool>> ToggleTop(long id)
    {
        var announcement = await _context.Announcements.FindAsync(id);
        if (announcement == null)
        {
            return ApiResponse<bool>.Fail("公告不存在");
        }

        announcement.IsTop = !announcement.IsTop;
        announcement.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, announcement.IsTop ? "已置顶" : "已取消置顶");
    }
}

public class AnnouncementDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = "notice";
    public string Status { get; set; } = "draft";
    public bool IsTop { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsRead { get; set; }
}

public class CreateAnnouncementRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Status { get; set; }
    public bool IsTop { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}

public class UpdateAnnouncementRequest
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public bool? IsTop { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}
