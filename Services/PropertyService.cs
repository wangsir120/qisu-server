using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models;
using qisu_server.Models.DTOs;

namespace qisu_server.Services;

public interface IPropertyService
{
    Task<ApiResponse<PagedResult<PropertyListDto>>> GetListAsync(long hostId, PropertyQueryRequest request);
    Task<ApiResponse<PropertyListDto>> GetByIdAsync(long hostId, long id);
    Task<ApiResponse<PropertyListDto>> CreateAsync(long hostId, PropertyCreateRequest request);
    Task<ApiResponse<PropertyListDto>> UpdateAsync(long hostId, long id, PropertyUpdateRequest request);
    Task<ApiResponse<bool>> DeleteAsync(long hostId, long id);
    Task<ApiResponse<bool>> BatchDeleteAsync(long hostId, List<long> ids);
}

public class PropertyService : IPropertyService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PropertyService> _logger;

    public PropertyService(AppDbContext context, ILogger<PropertyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<PropertyListDto>>> GetListAsync(long hostId, PropertyQueryRequest request)
    {
        var query = _context.Properties
            .Where(p => p.HostId == hostId);

        if (!string.IsNullOrEmpty(request.Title))
        {
            query = query.Where(p => p.Title.Contains(request.Title));
        }
        if (!string.IsNullOrEmpty(request.PropertyType))
        {
            query = query.Where(p => p.PropertyType == request.PropertyType);
        }
        if (!string.IsNullOrEmpty(request.Status))
        {
            query = request.Status switch
            {
                "available" => query.Where(p => p.Status == 1),
                "occupied" => query.Where(p => p.Status == 2),
                "maintenance" => query.Where(p => p.Status == 3),
                _ => byte.TryParse(request.Status, out var s) ? query.Where(p => p.Status == s) : query
            };
        }

        var total = await query.CountAsync();

        var rawItems = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new
            {
                p.Id,
                p.HostId,
                p.Title,
                p.Description,
                p.PropertyType,
                p.Area,
                p.BedType,
                p.MaxGuests,
                p.Bedrooms,
                RoomCount = p.RoomCount ?? 0,
                p.AddressId,
                AddressName = p.PropertyAddress != null ? p.PropertyAddress.FullAddress : null,
                p.PricePerNight,
                p.Status,
                p.IsInstantBook,
                p.IsNew,
                p.Facilities,
                p.CreatedAt,
                p.UpdatedAt,
                CoverImage = p.Images.Where(i => i.IsCover).Select(i => i.ImageUrl).FirstOrDefault()
                    ?? p.Images.Select(i => i.ImageUrl).FirstOrDefault(),
                Images = p.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).ToList()
            })
            .ToListAsync();

        var items = rawItems.Select(p => new PropertyListDto
        {
            Id = p.Id,
            HostId = p.HostId,
            Title = p.Title,
            Description = p.Description,
            PropertyType = p.PropertyType,
            Area = p.Area,
            BedType = p.BedType,
            MaxGuests = p.MaxGuests,
            Bedrooms = p.Bedrooms,
            RoomCount = p.RoomCount,
            AddressId = p.AddressId,
            AddressName = p.AddressName,
            PricePerNight = p.PricePerNight,
            Status = p.Status,
            IsInstantBook = p.IsInstantBook,
            IsNew = p.IsNew,
            Facilities = p.Facilities != null ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(p.Facilities) : null,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            CoverImage = p.CoverImage,
            Images = p.Images
        }).ToList();

        return ApiResponse<PagedResult<PropertyListDto>>.Ok(new PagedResult<PropertyListDto>
        {
            Items = items,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)request.PageSize)
        });
    }

    public async Task<ApiResponse<PropertyListDto>> GetByIdAsync(long hostId, long id)
    {
        var property = await _context.Properties
            .Include(p => p.PropertyAddress)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id && p.HostId == hostId);

        if (property == null)
        {
            return ApiResponse<PropertyListDto>.Fail("房源不存在");
        }

        return ApiResponse<PropertyListDto>.Ok(MapToListDto(property));
    }

    public async Task<ApiResponse<PropertyListDto>> CreateAsync(long hostId, PropertyCreateRequest request)
    {
        var property = new Property
        {
            HostId = hostId,
            Title = request.Title,
            Description = request.Description,
            PropertyType = request.PropertyType,
            Area = request.Area,
            BedType = request.BedType,
            MaxGuests = request.MaxGuests,
            Bedrooms = request.Bedrooms,
            RoomCount = request.Bedrooms,
            AddressId = request.AddressId,
            PricePerNight = request.PricePerNight,
            Facilities = request.Facilities != null ? System.Text.Json.JsonSerializer.Serialize(request.Facilities) : null,
            IsInstantBook = request.IsInstantBook,
            IsNew = request.IsNew,
            Status = 1,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _context.Properties.Add(property);
        await _context.SaveChangesAsync();

        if (request.Images != null && request.Images.Count > 0)
        {
            var images = request.Images.Select((img, index) => new PropertyImage
            {
                PropertyId = property.Id,
                ImageUrl = img.Url,
                IsCover = img.IsCover,
                SortOrder = index,
                CreatedAt = DateTime.Now
            }).ToList();
            _context.PropertyImages.AddRange(images);
            await _context.SaveChangesAsync();
        }

        // 根据房间数和楼层自动生成房间
        var roomCount = request.Bedrooms;
        var floor = request.Floor;
        if (roomCount > 0)
        {
            var rooms = Enumerable.Range(0, roomCount).Select(i => new Room
            {
                PropertyId = property.Id,
                Name = $"{floor * 1000 + i}",
                RoomType = request.PropertyType,
                Area = request.Area,
                BedType = request.BedType,
                Beds = 1,
                MaxGuests = request.MaxGuests,
                PricePerNight = request.PricePerNight,
                Floor = floor,
                Status = 1,
                Facilities = request.Facilities != null ? System.Text.Json.JsonSerializer.Serialize(request.Facilities) : null,
                Description = request.Description,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }).ToList();
            _context.Rooms.AddRange(rooms);
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("房东 {HostId} 创建房源 {PropertyId}", hostId, property.Id);

        await RefreshHostListings(hostId);

        return await GetByIdAsync(hostId, property.Id);
    }

    public async Task<ApiResponse<PropertyListDto>> UpdateAsync(long hostId, long id, PropertyUpdateRequest request)
    {
        var property = await _context.Properties.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id && p.HostId == hostId);
        if (property == null)
        {
            return ApiResponse<PropertyListDto>.Fail("房源不存在");
        }

        if (request.Title != null) property.Title = request.Title;
        if (request.Description != null) property.Description = request.Description;
        if (request.PropertyType != null) property.PropertyType = request.PropertyType;
        if (request.Area.HasValue) property.Area = request.Area.Value;
        if (request.BedType != null) property.BedType = request.BedType;
        if (request.MaxGuests.HasValue) property.MaxGuests = request.MaxGuests.Value;
        if (request.Bedrooms.HasValue)
        {
            property.Bedrooms = request.Bedrooms.Value;
            property.RoomCount = request.Bedrooms.Value;
        }
        if (request.AddressId.HasValue) property.AddressId = request.AddressId.Value;
        if (request.PricePerNight.HasValue) property.PricePerNight = request.PricePerNight.Value;
        if (request.Facilities != null) property.Facilities = System.Text.Json.JsonSerializer.Serialize(request.Facilities);
        if (request.IsInstantBook.HasValue) property.IsInstantBook = request.IsInstantBook.Value;
        if (request.IsNew.HasValue) property.IsNew = request.IsNew.Value;
        if (request.Status.HasValue) property.Status = request.Status.Value;

        property.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        if (request.Images != null)
        {
            var existingImages = await _context.PropertyImages.Where(i => i.PropertyId == id).ToListAsync();
            _context.PropertyImages.RemoveRange(existingImages);

            if (request.Images.Count > 0)
            {
                var newImages = request.Images.Select((img, index) => new PropertyImage
                {
                    PropertyId = id,
                    ImageUrl = img.Url,
                    IsCover = img.IsCover,
                    SortOrder = index,
                    CreatedAt = DateTime.Now
                }).ToList();
                _context.PropertyImages.AddRange(newImages);
            }

            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("房东 {HostId} 更新房源 {PropertyId}", hostId, id);

        return await GetByIdAsync(hostId, id);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(long hostId, long id)
    {
        var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == id && p.HostId == hostId);
        if (property == null)
        {
            return ApiResponse<bool>.Fail("房源不存在");
        }

        _context.Properties.Remove(property);
        await _context.SaveChangesAsync();
        await RefreshHostListings(hostId);

        _logger.LogInformation("房东 {HostId} 删除房源 {PropertyId}", hostId, id);

        return ApiResponse<bool>.Ok(true, "删除成功");
    }

    public async Task<ApiResponse<bool>> BatchDeleteAsync(long hostId, List<long> ids)
    {
        var properties = await _context.Properties
            .Where(p => p.HostId == hostId && ids.Contains(p.Id))
            .ToListAsync();

        if (properties.Count == 0)
        {
            return ApiResponse<bool>.Fail("未找到可删除的房源");
        }

        _context.Properties.RemoveRange(properties);
        await _context.SaveChangesAsync();
        await RefreshHostListings(hostId);

        _logger.LogInformation("房东 {HostId} 批量删除 {Count} 个房源", hostId, properties.Count);

        return ApiResponse<bool>.Ok(true, $"成功删除 {properties.Count} 个房源");
    }

    private async Task RefreshHostListings(long hostId)
    {
        var host = await _context.Hosts.FindAsync(hostId);
        if (host != null)
        {
            host.TotalListings = await _context.Properties.CountAsync(p => p.HostId == hostId);
            await _context.SaveChangesAsync();
        }
    }

    private static PropertyListDto MapToListDto(Property p)
    {
        return new PropertyListDto
        {
            Id = p.Id,
            HostId = p.HostId,
            Title = p.Title,
            Description = p.Description,
            PropertyType = p.PropertyType,
            Area = p.Area,
            BedType = p.BedType,
            MaxGuests = p.MaxGuests,
            Bedrooms = p.Bedrooms,
            RoomCount = p.RoomCount ?? 0,
            AddressId = p.AddressId,
            AddressName = p.PropertyAddress?.FullAddress,
            PricePerNight = p.PricePerNight,
            Status = p.Status,
            IsInstantBook = p.IsInstantBook,
            IsNew = p.IsNew,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            CoverImage = p.Images?.Where(i => i.IsCover).Select(i => i.ImageUrl).FirstOrDefault()
                ?? p.Images?.Select(i => i.ImageUrl).FirstOrDefault(),
            Images = p.Images?.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).ToList(),
            Facilities = p.Facilities != null ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(p.Facilities) : null
        };
    }
}
