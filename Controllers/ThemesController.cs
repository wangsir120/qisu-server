using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models.DTOs;

namespace qisu_server.Controllers;

/// <summary>
/// 主题分类控制器
/// </summary>
[ApiController]
[Route("api/themes")]
public class ThemesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ThemesController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取主题列表
    /// </summary>
    /// <returns>所有启用的主题，按排序值升序排列</returns>
    [HttpGet]
    public async Task<ApiResponse<List<ThemeDto>>> GetList()
    {
        var items = await _context.Themes
            .Where(t => t.Status)
            .OrderBy(t => t.SortOrder)
            .ThenByDescending(t => t.CreatedAt)
            .Select(t => new ThemeDto
            {
                Id = t.Id,
                Name = t.Name,
                ImageUrl = t.ImageUrl,
                Description = t.Description,
                PropertyCount = t.PropertyCount,
                SortOrder = t.SortOrder
            })
            .ToListAsync();

        return ApiResponse<List<ThemeDto>>.Ok(items);
    }
}

public class ThemeDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public int PropertyCount { get; set; }
    public int SortOrder { get; set; }
}
