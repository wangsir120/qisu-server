using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models;
using qisu_server.Models.DTOs;

namespace qisu_server.Controllers;

/// <summary>
/// 公开轮播图接口控制器（无需登录即可访问）
/// </summary>
[ApiController]
[Route("api/banners")]
public class PublicBannersController : ControllerBase
{
    private readonly AppDbContext _context;

    public PublicBannersController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取启用的轮播图列表
    /// </summary>
    /// <param name="position">位置筛选：home-首页</param>
    /// <returns>轮播图列表，按排序值升序排列</returns>
    /// <remarks>
    /// 只返回启用状态且在有效期内的轮播图
    /// </remarks>
    [HttpGet]
    public async Task<ApiResponse<List<BannerDto>>> GetList([FromQuery] string? position = null)
    {
        var now = DateTime.Now;
        
        var query = _context.Banners
            .Where(b => b.Status == true)
            .Where(b => !b.StartTime.HasValue || b.StartTime <= now)
            .Where(b => !b.EndTime.HasValue || b.EndTime >= now);

        if (!string.IsNullOrEmpty(position))
        {
            query = query.Where(b => b.Position == position);
        }

        var items = await query
            .OrderBy(b => b.SortOrder)
            .ThenByDescending(b => b.CreatedAt)
            .Select(b => new BannerDto
            {
                Id = b.Id,
                Title = b.Title,
                Subtitle = b.Subtitle,
                ImageUrl = b.ImageUrl,
                LinkUrl = b.LinkUrl,
                Gradient = b.Gradient,
                Position = b.Position
            })
            .ToListAsync();

        return ApiResponse<List<BannerDto>>.Ok(items);
    }
}
