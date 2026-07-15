using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models;

namespace qisu_server.Controllers;

/// <summary>
/// 旅行故事公开接口控制器（无需登录即可访问）
/// </summary>
[ApiController]
[Route("api/travel-stories")]
public class PublicTravelStoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public PublicTravelStoriesController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取旅行故事列表
    /// </summary>
    /// <param name="storyType">故事类型筛选</param>
    /// <param name="limit">返回数量限制，默认10条</param>
    /// <returns>旅行故事列表，按创建时间倒序排列</returns>
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] string? storyType = null,
        [FromQuery] int limit = 10)
    {
        var query = _context.TravelStories
            .Where(s => s.Status == true);

        if (!string.IsNullOrEmpty(storyType))
        {
            query = query.Where(s => s.StoryType == storyType);
        }

        var stories = await query
            .OrderByDescending(s => s.CreatedAt)
            .Take(limit)
            .Select(s => new
            {
                s.Id,
                s.Title,
                s.Content,
                s.ImageUrl,
                s.StoryType,
                s.ViewCount,
                s.LikeCount,
                s.CreatedAt,
                Author = s.User != null ? s.User.Nickname ?? "匿名用户" : "栖宿旅行"
            })
            .ToListAsync();

        return Ok(new { code = 200, data = stories, success = true });
    }

    /// <summary>
    /// 获取旅行故事详情
    /// </summary>
    /// <param name="id">故事ID</param>
    /// <returns>故事详情信息，访问后自动增加浏览量</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var story = await _context.TravelStories
            .Where(s => s.Id == id && s.Status == true)
            .Select(s => new
            {
                s.Id,
                s.Title,
                s.Content,
                s.ImageUrl,
                s.StoryType,
                s.ViewCount,
                s.LikeCount,
                s.CreatedAt,
                Author = s.User != null ? s.User.Nickname ?? "匿名用户" : "栖宿旅行",
                AuthorAvatar = s.User != null ? s.User.Avatar : null
            })
            .FirstOrDefaultAsync();

        if (story == null)
        {
            return NotFound(new { code = 404, message = "旅行故事不存在", success = false });
        }

        var entity = await _context.TravelStories.FindAsync(id);
        if (entity != null)
        {
            entity.ViewCount++;
            await _context.SaveChangesAsync();
        }

        return Ok(new { code = 200, data = story, success = true });
    }
}
