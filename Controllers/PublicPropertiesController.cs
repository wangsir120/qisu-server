using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models;
using qisu_server.Models.DTOs;

namespace qisu_server.Controllers;

/// <summary>
/// 公开房源接口控制器（无需登录即可访问）
/// </summary>
[ApiController]
[Route("api/properties")]
public class PublicPropertiesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<PublicPropertiesController> _logger;

    public PublicPropertiesController(AppDbContext context, ILogger<PublicPropertiesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取推荐房源列表（支持地理位置筛选）
    /// </summary>
    /// <param name="limit">返回数量限制，默认8条</param>
    /// <param name="latitude">用户纬度（可选，用于计算距离）</param>
    /// <param name="longitude">用户经度（可选，用于计算距离）</param>
    /// <returns>按距离或评分排序的推荐房源</returns>
    [HttpGet("recommended")]
    public async Task<ApiResponse<List<PropertyDto>>> GetRecommended(
        [FromQuery] int limit = 8,
        [FromQuery] decimal? latitude = null,
        [FromQuery] decimal? longitude = null)
    {
        var query = _context.Properties
            .Include(p => p.Host)
            .Include(p => p.Images)
            .Include(p => p.PropertyAddress)
            .Where(p => p.Status == 1
                && p.PropertyAddress != null
                && p.PropertyAddress.Latitude.HasValue
                && p.PropertyAddress.Longitude.HasValue);

        List<Property> properties;

        if (latitude.HasValue && longitude.HasValue)
        {
            var userLat = (double)latitude.Value;
            var userLng = (double)longitude.Value;

            properties = await query.ToListAsync();

            var propertiesWithDistance = properties.Select(p =>
            {
                var distance = GeoHelper.CalculateDistance(
                    userLat, userLng,
                    (double)p.PropertyAddress!.Latitude!.Value,
                    (double)p.PropertyAddress!.Longitude!.Value);
                return new { Property = p, Distance = distance };
            })
            .OrderBy(x => x.Distance)
            .ThenByDescending(x => x.Property.Rating)
            .Take(limit)
            .ToList();

            _logger.LogInformation($"附近推荐：用户位置({userLat}, {userLng})，返回{propertiesWithDistance.Count}条房源");
            properties = propertiesWithDistance.Select(x => x.Property).ToList();
        }
        else
        {
            properties = await query
                .OrderByDescending(p => p.Rating)
                .ThenByDescending(p => p.ReviewCount)
                .ThenByDescending(p => p.FavoriteCount)
                .Take(limit)
                .ToListAsync();

            _logger.LogInformation($"默认推荐：无定位信息，返回{properties.Count}条高评分房源");
        }

        var dtos = properties.Select(MapToDto).ToList();

        return ApiResponse<List<PropertyDto>>.Ok(dtos);
    }

    /// <summary>
    /// 获取精选房源列表
    /// </summary>
    /// <param name="limit">返回数量限制，默认4条</param>
    /// <returns>包含闪订、新上、超赞房东等标签的精选房源</returns>
    [HttpGet("featured")]
    public async Task<ApiResponse<List<PropertyDto>>> GetFeatured([FromQuery] int limit = 4)
    {
        var properties = await _context.Properties
            .Include(p => p.Host)
            .Include(p => p.Images)
            .Include(p => p.PropertyAddress)
            .Where(p => p.Status == 1 && (p.IsInstantBook || p.IsNew || (p.Host != null && p.Host.IsSuperhost)))
            .OrderByDescending(p => p.Rating)
            .ThenByDescending(p => p.ReviewCount)
            .ThenByDescending(p => p.FavoriteCount)
            .Take(limit)
            .ToListAsync();

        var dtos = properties.Select(MapToDto).ToList();

        return ApiResponse<List<PropertyDto>>.Ok(dtos);
    }

    /// <summary>
    /// 获取房源列表（支持筛选和排序）
    /// </summary>
    /// <param name="page">页码，默认1</param>
    /// <param name="pageSize">每页数量，默认12</param>
    /// <param name="keyword">搜索关键词（可匹配标题、地区、城市、类型等）</param>
    /// <param name="city">城市筛选</param>
    /// <param name="district">区域筛选</param>
    /// <param name="sortBy">排序方式：recommend-推荐（默认），price_asc-价格升序，price_desc-价格降序，rating-评分</param>
    /// <param name="minPrice">最低价格</param>
    /// <param name="maxPrice">最高价格</param>
    /// <param name="minRating">最低评分</param>
    /// <param name="isNew">是否仅筛选新上线房源</param>
    /// <param name="themeId">主题ID，用于筛选特定主题下的房源</param>
    /// <returns>分页的房源列表</returns>
    [HttpGet("list")]
    public async Task<ApiResponse<PagedResult<PropertyDto>>> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? keyword = null,
        [FromQuery] string? city = null,
        [FromQuery] string? district = null,
        [FromQuery] string? sortBy = "recommend",
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] int? minRating = null,
        [FromQuery] bool? isNew = null,
        [FromQuery] long? themeId = null)
    {
        var query = _context.Properties
            .Include(p => p.Host)
            .Include(p => p.Images)
            .Include(p => p.PropertyAddress)
            .Where(p => p.Status == 1);

        if (!string.IsNullOrEmpty(keyword))
        {
            var searchKeyword = keyword.Trim().ToLower();

            query = query.Where(p =>
                (p.Title != null && p.Title.ToLower().Contains(searchKeyword)) ||
                (p.PropertyType != null && p.PropertyType.ToLower().Contains(searchKeyword)) ||
                (p.Description != null && p.Description.ToLower().Contains(searchKeyword)) ||
                (p.PropertyAddress != null && p.PropertyAddress.City != null && p.PropertyAddress.City.ToLower().Contains(searchKeyword)) ||
                (p.PropertyAddress != null && p.PropertyAddress.District != null && p.PropertyAddress.District.ToLower().Contains(searchKeyword)) ||
                (p.PropertyAddress != null && p.PropertyAddress.FullAddress != null && p.PropertyAddress.FullAddress.ToLower().Contains(searchKeyword))
            );
        }

        if (!string.IsNullOrEmpty(city))
        {
            query = query.Where(p => p.PropertyAddress != null && p.PropertyAddress.City != null && p.PropertyAddress.City.Contains(city));
        }

        if (!string.IsNullOrEmpty(district))
        {
            query = query.Where(p => p.PropertyAddress != null && p.PropertyAddress.District != null && p.PropertyAddress.District.Contains(district));
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.PricePerNight >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.PricePerNight <= maxPrice.Value);
        }

        if (minRating.HasValue)
        {
            query = query.Where(p => p.Rating >= minRating.Value);
        }

        if (isNew.HasValue && isNew.Value)
        {
            query = query.Where(p => p.IsNew == true);
        }

        if (themeId.HasValue)
        {
            query = query.Where(p => p.PropertyThemes.Any(pt => pt.ThemeId == themeId.Value));
        }

        query = sortBy switch
        {
            "price_asc" => query.OrderBy(p => p.PricePerNight),
            "price_desc" => query.OrderByDescending(p => p.PricePerNight),
            "rating" => query.OrderByDescending(p => p.Rating).ThenByDescending(p => p.ReviewCount),
            _ => query.OrderByDescending(p => p.Rating)
                .ThenByDescending(p => p.ReviewCount)
                .ThenByDescending(p => p.FavoriteCount)
        };

        var totalCount = await query.CountAsync();
        var properties = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = properties.Select(MapToDto).ToList();

        var result = new PagedResult<PropertyDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return ApiResponse<PagedResult<PropertyDto>>.Ok(result);
    }

    /// <summary>
    /// 获取房源详情
    /// </summary>
    /// <param name="id">房源ID</param>
    /// <returns>房源详细信息，包含房东信息、设施、评价、评分分布等</returns>
    [HttpGet("{id}")]
    public async Task<ApiResponse<PropertyDetailDto>> GetById(long id)
    {
        var property = await _context.Properties
            .Include(p => p.Host)
            .Include(p => p.Images)
            .Include(p => p.PropertyAddress)
            .FirstOrDefaultAsync(p => p.Id == id && p.Status == 1);

        if (property == null)
        {
            return ApiResponse<PropertyDetailDto>.Fail("房源不存在");
        }

        await _context.Properties
            .Where(p => p.Id == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.ViewCount, p => p.ViewCount + 1));

        property.ViewCount += 1;

        var allReviews = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Images)
            .Include(r => r.Replies)
            .ThenInclude(rr => rr.Host)
            .Include(r => r.Replies)
            .ThenInclude(rr => rr.User)
            .Where(r => r.PropertyId == id && r.Status)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var tags = new List<string>();
        if (property.IsInstantBook) tags.Add("闪订");
        if (property.IsNew) tags.Add("新上");
        if (property.Host != null && property.Host.IsSuperhost) tags.Add("超赞房东");
        if (property.Rating >= 4.8m) tags.Add("高分房源");
        if (tags.Count == 0) tags.Add("精选房源");

        var facilities = GenerateFacilities(property);

        var dto = new PropertyDetailDto
        {
            Id = property.Id,
            Title = property.Title,
            Description = property.Description,
            PropertyType = property.PropertyType,
            Address = property.PropertyAddress?.FullAddress,
            City = property.PropertyAddress?.City,
            District = property.PropertyAddress?.District,
            Location = property.PropertyAddress?.FullAddress,
            Latitude = property.PropertyAddress?.Latitude,
            Longitude = property.PropertyAddress?.Longitude,
            Bedrooms = property.Bedrooms,
            Beds = property.Beds,
            Bathrooms = property.Bathrooms,
            MaxGuests = property.MaxGuests,
            RoomCount = property.RoomCount ?? 0,
            AddressId = property.AddressId,
            PricePerNight = property.PricePerNight,
            CleaningFee = property.CleaningFee,
            ServiceFeeRate = property.ServiceFeeRate,
            Rating = property.Rating,
            ReviewCount = allReviews.Count,
            ViewCount = property.ViewCount,
            FavoriteCount = property.FavoriteCount,
            IsInstantBook = property.IsInstantBook,
            IsNew = property.IsNew,
            HostId = property.HostId,
            HostName = property.Host != null ? property.Host.Name : "",
            HostAvatar = property.Host != null ? property.Host.Avatar : null,
            HostPhone = property.Host != null ? property.Host.Phone : null,
            IsSuperhost = property.Host != null && property.Host.IsSuperhost,
            ResponseRate = property.Host != null ? property.Host.ResponseRate : null,
            ResponseTime = property.Host != null ? property.Host.ResponseTime : null,
            TotalListings = property.Host != null ? property.Host.TotalListings : 0,
            TotalReviews = allReviews.Count,
            CoverImage = property.Images != null && property.Images.Any()
                ? (property.Images.FirstOrDefault(i => i.IsCover) != null
                    ? property.Images.FirstOrDefault(i => i.IsCover)!.ImageUrl
                    : property.Images.First().ImageUrl)
                : null,
            Images = property.Images != null
                ? property.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).ToList()
                : new List<string>(),
            CreatedAt = property.CreatedAt,
            UpdatedAt = property.UpdatedAt,
            Facilities = facilities,
            Reviews = allReviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = r.IsAnonymous ? "匿名用户" : (r.User != null ? (r.User.Nickname ?? r.User.Username ?? "用户") : "用户"),
                UserAvatar = r.IsAnonymous ? null : (r.User?.Avatar),
                Rating = (int)r.Rating,
                Content = r.Content ?? "",
                CreatedAt = r.CreatedAt,
                Images = r.Images != null ? r.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).ToList() : new List<string>(),
                HostReply = r.HostReply,
                HostReplyTime = r.HostReplyTime,
                Replies = r.Replies != null ? r.Replies.OrderBy(rr => rr.CreatedAt).Select(rr =>
                {
                    var isHostReply = rr.HostId.HasValue && rr.HostId.Value > 0;
                    return new PropertyReviewReplyDto
                    {
                        Id = rr.Id,
                        UserId = isHostReply ? (rr.Host?.UserId ?? 0) : (rr.UserId ?? 0),
                        UserName = isHostReply
                            ? (rr.Host?.Name ?? "房东")
                            : (rr.User != null ? (rr.User.Nickname ?? rr.User.Username ?? "用户") : "用户"),
                        UserAvatar = isHostReply ? rr.Host?.Avatar : rr.User?.Avatar,
                        UserRole = isHostReply ? "host" : "guest",
                        Content = rr.Content,
                        CreatedAt = rr.CreatedAt
                    };
                }).ToList() : new()
            }).ToList(),
            RatingDistribution = CalculateRatingDistribution(allReviews),
            Tags = tags,
            CancellationPolicy = "入住前1天可免费取消",
            Rules = new List<string>
            {
                "入住时间：14:00后",
                "退房时间：12:00前",
                "不允许举办派对",
                "禁止吸烟"
            }
        };

        return ApiResponse<PropertyDetailDto>.Ok(dto);
    }

    internal static PropertyDto MapToDto(Property p)
    {
        return new PropertyDto
        {
            Id = p.Id,
            Title = p.Title,
            Description = p.Description,
            PropertyType = p.PropertyType,
            Address = p.PropertyAddress?.FullAddress,
            City = p.PropertyAddress?.City,
            District = p.PropertyAddress?.District,
            Location = p.PropertyAddress?.FullAddress,
            Latitude = p.PropertyAddress?.Latitude,
            Longitude = p.PropertyAddress?.Longitude,
            Bedrooms = p.Bedrooms,
            Beds = p.Beds,
            Bathrooms = p.Bathrooms,
            MaxGuests = p.MaxGuests,
            RoomCount = p.RoomCount ?? 0,
            AddressId = p.AddressId,
            PricePerNight = p.PricePerNight,
            Rating = p.Rating,
            ReviewCount = p.ReviewCount,
            ViewCount = p.ViewCount,
            FavoriteCount = p.FavoriteCount,
            IsInstantBook = p.IsInstantBook,
            IsNew = p.IsNew,
            HostName = p.Host != null ? p.Host.Name : "",
            IsSuperhost = p.Host != null && p.Host.IsSuperhost,
            CoverImage = p.Images != null && p.Images.Any()
                ? (p.Images.FirstOrDefault(i => i.IsCover) != null
                    ? p.Images.FirstOrDefault(i => i.IsCover)!.ImageUrl
                    : p.Images.First().ImageUrl)
                : null,
            Images = p.Images != null
                ? p.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).ToList()
                : new List<string>(),
            CreatedAt = p.CreatedAt
        };
    }

    [HttpGet("{id}/orders/check")]
    public async Task<ApiResponse<object>> CheckOrderAvailability(long id, [FromQuery] string date)
    {
        if (!DateOnly.TryParse(date, out var checkDate))
        {
            return ApiResponse<object>.Fail("日期格式不正确");
        }

        var property = await _context.Properties
            .Where(p => p.Id == id && p.Status == 1)
            .Select(p => new { RoomCount = p.RoomCount ?? 0 })
            .FirstOrDefaultAsync();

        if (property == null)
        {
            return ApiResponse<object>.Fail("房源不存在");
        }

        var cancelledStatuses = new[] { "cancelled", "rejected" };
        var startOfDay = checkDate.ToDateTime(TimeOnly.MinValue);
        var endOfDay = checkDate.ToDateTime(TimeOnly.MaxValue);

        var orderCount = await _context.Orders
            .Where(o => o.PropertyId == id
                && o.CheckInDate >= startOfDay
                && o.CheckInDate <= endOfDay
                && !cancelledStatuses.Contains(o.Status))
            .CountAsync();

        return ApiResponse<object>.Ok(new
        {
            propertyId = id,
            roomCount = property.RoomCount,
            orderCount,
            available = orderCount < property.RoomCount
        });
    }

    private static List<FacilityDto> GenerateFacilities(Property property)
    {
        var facilities = new List<FacilityDto>
        {
            new() { Icon = "wifi", Name = "无线网络", Available = true },
            new() { Icon = "tv", Name = "电视", Available = true },
            new() { Icon = "aircon", Name = "空调", Available = true },
            new() { Icon = "washer", Name = "洗衣机", Available = property.MaxGuests <= 4 }
        };
        if (property.PricePerNight >= 200)
        {
            facilities.Add(new() { Icon = "kitchen", Name = "可做饭", Available = true });
            facilities.Add(new() { Icon = "parking", Name = "免费停车", Available = true });
        }
        if (property.PricePerNight >= 400)
        {
            facilities.Add(new() { Icon = "bathtub", Name = "热水浴缸", Available = property.Bedrooms >= 2 });
        }
        if (property.MaxGuests >= 4)
        {
            facilities.Add(new() { Icon = "pet", Name = "允许宠物", Available = false });
        }
        return facilities;
    }

    private static RatingDistributionDto CalculateRatingDistribution(List<Review> reviews)
    {
        if (reviews == null || reviews.Count == 0)
        {
            return new RatingDistributionDto();
        }
        return new RatingDistributionDto
        {
            Cleanliness = Math.Round(reviews.Where(r => r.CleanlinessRating.HasValue).Average(r => r.CleanlinessRating!.Value), 1),
            Communication = Math.Round(reviews.Where(r => r.CommunicationRating.HasValue).Average(r => r.CommunicationRating!.Value), 1),
            CheckIn = Math.Round(reviews.Where(r => r.CheckinRating.HasValue).Average(r => r.CheckinRating!.Value), 1),
            Accuracy = Math.Round(reviews.Where(r => r.AccuracyRating.HasValue).Average(r => r.AccuracyRating!.Value), 1),
            Location = Math.Round(reviews.Where(r => r.LocationRating.HasValue).Average(r => r.LocationRating!.Value), 1),
            Value = Math.Round(reviews.Where(r => r.ValueRating.HasValue).Average(r => r.ValueRating!.Value), 1)
        };
    }
}

public class PropertyDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PropertyType { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Location { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int Bedrooms { get; set; }
    public int Beds { get; set; }
    public int Bathrooms { get; set; }
    public int MaxGuests { get; set; }
    public int RoomCount { get; set; }
    public long? AddressId { get; set; }
    public decimal PricePerNight { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public int ViewCount { get; set; }
    public int FavoriteCount { get; set; }
    public bool IsInstantBook { get; set; }
    public bool IsNew { get; set; }
    public string HostName { get; set; } = string.Empty;
    public bool IsSuperhost { get; set; }
    public string? CoverImage { get; set; }
    public List<string> Images { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class PropertyDetailDto : PropertyDto
{
    public long HostId { get; set; }
    public string? HostAvatar { get; set; }
    public string? HostPhone { get; set; }
    public decimal? CleaningFee { get; set; }
    public decimal? ServiceFeeRate { get; set; }
    public decimal? ResponseRate { get; set; }
    public string? ResponseTime { get; set; }
    public int TotalListings { get; set; }
    public int TotalReviews { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<FacilityDto> Facilities { get; set; } = new();
    public List<ReviewDto> Reviews { get; set; } = new();
    public RatingDistributionDto RatingDistribution { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public string? CancellationPolicy { get; set; }
    public List<string> Rules { get; set; } = new();
}

public class FacilityDto
{
    public string Icon { get; set; } = "wifi";
    public string Name { get; set; } = "";
    public bool Available { get; set; } = true;
}

public class ReviewDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string UserName { get; set; } = "";
    public string? UserAvatar { get; set; }
    public int Rating { get; set; }
    public string Content { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public List<string> Images { get; set; } = new();
    public string? HostReply { get; set; }
    public DateTime? HostReplyTime { get; set; }
    public List<PropertyReviewReplyDto> Replies { get; set; } = new();
}

public class PropertyReviewReplyDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string UserName { get; set; } = "";
    public string? UserAvatar { get; set; }
    public string UserRole { get; set; } = "guest";
    public string Content { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class RatingDistributionDto
{
    public decimal Cleanliness { get; set; } = 5.0m;
    public decimal Communication { get; set; } = 5.0m;
    public decimal CheckIn { get; set; } = 5.0m;
    public decimal Accuracy { get; set; } = 5.0m;
    public decimal Location { get; set; } = 5.0m;
    public decimal Value { get; set; } = 5.0m;
}

public static class GeoHelper
{
    private const double EarthRadiusKm = 6371.0;

    public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
    }

    private static double ToRadians(double angle)
    {
        return angle * (Math.PI / 180);
    }
}
