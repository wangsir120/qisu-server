using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models.DTOs;

namespace qisu_server.Controllers;

[ApiController]
[Route("api/destinations")]
public class HotDestinationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public HotDestinationsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ApiResponse<List<HotDestinationDto>>> GetList(
        [FromQuery] string? sortBy = "hot_score",
        [FromQuery] int limit = 7)
    {
        var destinations = await _context.HotDestinations
            .Where(d => d.Status && d.PropertyCount > 0)
            .OrderByDescending(d => d.HotScore)
            .Take(limit)
            .Select(d => new HotDestinationDto
            {
                Id = d.Id,
                Name = d.Name,
                Image = d.Image,
                PropertyCount = d.PropertyCount,
                SortOrder = d.SortOrder,
                SearchCount = d.SearchCount,
                BookingCount = d.BookingCount,
                ViewCount = d.ViewCount,
                HotScore = d.HotScore
            })
            .ToListAsync();

        return ApiResponse<List<HotDestinationDto>>.Ok(destinations);
    }

    [HttpGet("{name}")]
    public async Task<ApiResponse<HotDestinationDetailDto>> GetByName(string name)
    {
        var dest = await _context.HotDestinations
            .FirstOrDefaultAsync(d => d.Name == name && d.Status);

        if (dest == null)
        {
            return ApiResponse<HotDestinationDetailDto>.Fail("目的地不存在");
        }

        var dto = new HotDestinationDetailDto
        {
            Id = dest.Id,
            Name = dest.Name,
            Image = dest.Image,
            PropertyCount = dest.PropertyCount,
            SortOrder = dest.SortOrder,
            SearchCount = dest.SearchCount,
            BookingCount = dest.BookingCount,
            ViewCount = dest.ViewCount,
            HotScore = dest.HotScore,
            Description = $"{dest.Name}是重庆的热门旅游目的地，拥有丰富的民宿资源和独特的山城文化体验。",
            Region = "重庆",
            Rating = 4.8m,
            BestTime = "春秋两季（3-5月，9-11月）气候宜人，最适合游览。",
            TrafficGuide = "可乘坐轻轨或公交车到达，建议提前预订周边民宿。"
        };

        return ApiResponse<HotDestinationDetailDto>.Ok(dto);
    }

    [HttpGet("{name}/properties")]
    public async Task<ApiResponse<PagedResult<PropertyDto>>> GetProperties(
        string name,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var decodedName = Uri.UnescapeDataString(name);

        var query = _context.Properties
            .Include(p => p.Host)
            .Include(p => p.Images)
            .Include(p => p.PropertyAddress)
            .Where(p => p.Status == 1)
            .Where(p =>
                p.Title.Contains(decodedName) ||
                (p.PropertyAddress != null && p.PropertyAddress.FullAddress != null && p.PropertyAddress.FullAddress.Contains(decodedName)));

        var total = await query.CountAsync();

        var properties = await query
            .OrderByDescending(p => p.Rating)
            .ThenByDescending(p => p.ReviewCount)
            .ThenByDescending(p => p.FavoriteCount)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = properties.Select(PublicPropertiesController.MapToDto).ToList();

        return ApiResponse<PagedResult<PropertyDto>>.Ok(new PagedResult<PropertyDto>
        {
            Items = dtos,
            Total = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        });
    }
}

public class HotDestinationDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Image { get; set; }
    public int PropertyCount { get; set; }
    public int SortOrder { get; set; }
    public int SearchCount { get; set; }
    public int BookingCount { get; set; }
    public int ViewCount { get; set; }
    public decimal HotScore { get; set; }
}

public class HotDestinationDetailDto : HotDestinationDto
{
    public string? Description { get; set; }
    public string? Region { get; set; }
    public decimal Rating { get; set; }
    public string? BestTime { get; set; }
    public string? TrafficGuide { get; set; }
}
