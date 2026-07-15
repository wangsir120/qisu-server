using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models;
using qisu_server.Models.DTOs;

namespace qisu_server.Controllers;

/// <summary>
/// 轮播图管理控制器
/// </summary>
[ApiController]
[Route("api/admin/banners")]
[Authorize]
public class BannersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<BannersController> _logger;

    public BannersController(AppDbContext context, ILogger<BannersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取轮播图列表
    /// </summary>
    /// <param name="page">页码，默认1</param>
    /// <param name="pageSize">每页数量，默认10</param>
    /// <param name="position">位置筛选：home-首页</param>
    /// <param name="status">状态筛选：true-启用，false-禁用</param>
    /// <returns>分页的轮播图列表</returns>
    [HttpGet]
    public async Task<ApiResponse<PagedResult<BannerDto>>> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? position = null,
        [FromQuery] bool? status = null)
    {
        var query = _context.Banners.AsQueryable();

        if (!string.IsNullOrEmpty(position))
        {
            query = query.Where(b => b.Position == position);
        }

        if (status.HasValue)
        {
            query = query.Where(b => b.Status == status.Value);
        }

        query = query.OrderBy(b => b.SortOrder).ThenByDescending(b => b.CreatedAt);

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BannerDto
            {
                Id = b.Id,
                Title = b.Title,
                Subtitle = b.Subtitle,
                ImageUrl = b.ImageUrl,
                LinkUrl = b.LinkUrl,
                LinkType = b.LinkType,
                Gradient = b.Gradient,
                Position = b.Position,
                SortOrder = b.SortOrder,
                Status = b.Status,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            })
            .ToListAsync();

        var result = new PagedResult<BannerDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<BannerDto>>.Ok(result);
    }

    /// <summary>
    /// 获取轮播图详情
    /// </summary>
    /// <param name="id">轮播图ID</param>
    /// <returns>轮播图详情信息</returns>
    [HttpGet("{id}")]
    public async Task<ApiResponse<BannerDto>> GetById(long id)
    {
        var banner = await _context.Banners.FindAsync(id);
        if (banner == null)
        {
            return ApiResponse<BannerDto>.Fail("轮播图不存在");
        }

        var dto = new BannerDto
        {
            Id = banner.Id,
            Title = banner.Title,
            Subtitle = banner.Subtitle,
            ImageUrl = banner.ImageUrl,
            LinkUrl = banner.LinkUrl,
            LinkType = banner.LinkType,
            Gradient = banner.Gradient,
            Position = banner.Position,
            SortOrder = banner.SortOrder,
            Status = banner.Status,
            StartTime = banner.StartTime,
            EndTime = banner.EndTime,
            CreatedAt = banner.CreatedAt,
            UpdatedAt = banner.UpdatedAt
        };

        return ApiResponse<BannerDto>.Ok(dto);
    }

    /// <summary>
    /// 创建轮播图
    /// </summary>
    /// <param name="request">轮播图创建请求</param>
    /// <returns>创建的轮播图信息</returns>
    [HttpPost]
    public async Task<ApiResponse<BannerDto>> Create([FromBody] CreateBannerRequest request)
    {
        var banner = new Banner
        {
            Title = request.Title,
            Subtitle = request.Subtitle,
            ImageUrl = request.ImageUrl,
            LinkUrl = request.LinkUrl,
            LinkType = request.LinkType,
            Gradient = request.Gradient,
            Position = request.Position ?? "home",
            SortOrder = request.SortOrder,
            Status = request.Status,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            CreatedAt = DateTime.Now
        };

        _context.Banners.Add(banner);
        await _context.SaveChangesAsync();

        var dto = new BannerDto
        {
            Id = banner.Id,
            Title = banner.Title,
            Subtitle = banner.Subtitle,
            ImageUrl = banner.ImageUrl,
            LinkUrl = banner.LinkUrl,
            LinkType = banner.LinkType,
            Gradient = banner.Gradient,
            Position = banner.Position,
            SortOrder = banner.SortOrder,
            Status = banner.Status,
            StartTime = banner.StartTime,
            EndTime = banner.EndTime,
            CreatedAt = banner.CreatedAt
        };

        return ApiResponse<BannerDto>.Ok(dto, "创建成功");
    }

    /// <summary>
    /// 更新轮播图信息
    /// </summary>
    /// <param name="id">轮播图ID</param>
    /// <param name="request">轮播图更新请求</param>
    /// <returns>更新结果</returns>
    [HttpPut("{id}")]
    public async Task<ApiResponse<bool>> Update(long id, [FromBody] UpdateBannerRequest request)
    {
        var banner = await _context.Banners.FindAsync(id);
        if (banner == null)
        {
            return ApiResponse<bool>.Fail("轮播图不存在");
        }

        if (request.Title != null)
            banner.Title = request.Title;
        if (request.Subtitle != null)
            banner.Subtitle = request.Subtitle;
        if (request.ImageUrl != null)
            banner.ImageUrl = request.ImageUrl;
        if (request.LinkUrl != null)
            banner.LinkUrl = request.LinkUrl;
        if (request.LinkType != null)
            banner.LinkType = request.LinkType;
        if (request.Gradient != null)
            banner.Gradient = request.Gradient;
        if (request.Position != null)
            banner.Position = request.Position;
        if (request.SortOrder.HasValue)
            banner.SortOrder = request.SortOrder.Value;
        if (request.Status.HasValue)
            banner.Status = request.Status.Value;
        if (request.StartTime.HasValue)
            banner.StartTime = request.StartTime;
        if (request.EndTime.HasValue)
            banner.EndTime = request.EndTime;

        banner.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "更新成功");
    }

    /// <summary>
    /// 删除轮播图
    /// </summary>
    /// <param name="id">轮播图ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    public async Task<ApiResponse<bool>> Delete(long id)
    {
        var banner = await _context.Banners.FindAsync(id);
        if (banner == null)
        {
            return ApiResponse<bool>.Fail("轮播图不存在");
        }

        _context.Banners.Remove(banner);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "删除成功");
    }

    /// <summary>
    /// 切换轮播图启用状态
    /// </summary>
    /// <param name="id">轮播图ID</param>
    /// <returns>操作结果</returns>
    [HttpPost("{id}/toggle-status")]
    public async Task<ApiResponse<bool>> ToggleStatus(long id)
    {
        var banner = await _context.Banners.FindAsync(id);
        if (banner == null)
        {
            return ApiResponse<bool>.Fail("轮播图不存在");
        }

        banner.Status = !banner.Status;
        banner.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, banner.Status ? "已启用" : "已禁用");
    }

    /// <summary>
    /// 批量更新轮播图排序
    /// </summary>
    /// <param name="sortItems">排序项列表，包含ID和排序值</param>
    /// <returns>更新结果</returns>
    [HttpPost("sort")]
    public async Task<ApiResponse<bool>> UpdateSort([FromBody] List<SortItem> sortItems)
    {
        foreach (var item in sortItems)
        {
            var banner = await _context.Banners.FindAsync(item.Id);
            if (banner != null)
            {
                banner.SortOrder = item.SortOrder;
                banner.UpdatedAt = DateTime.Now;
            }
        }

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "排序更新成功");
    }
}

/// <summary>
/// 轮播图数据传输对象
/// </summary>
public class BannerDto
{
    public long Id { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public string? LinkType { get; set; }
    public string? Gradient { get; set; }
    public string Position { get; set; } = "home";
    public int SortOrder { get; set; }
    public bool Status { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// 创建轮播图请求
/// </summary>
public class CreateBannerRequest
{
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public string? LinkType { get; set; }
    public string? Gradient { get; set; }
    public string? Position { get; set; }
    public int SortOrder { get; set; }
    public bool Status { get; set; } = true;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}

/// <summary>
/// 更新轮播图请求
/// </summary>
public class UpdateBannerRequest
{
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? ImageUrl { get; set; }
    public string? LinkUrl { get; set; }
    public string? LinkType { get; set; }
    public string? Gradient { get; set; }
    public string? Position { get; set; }
    public int? SortOrder { get; set; }
    public bool? Status { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}

/// <summary>
/// 排序项
/// </summary>
public class SortItem
{
    public long Id { get; set; }
    public int SortOrder { get; set; }
}
