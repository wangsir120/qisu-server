using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models;
using qisu_server.Models.DTOs;

namespace qisu_server.Services;

public interface IRoomService
{
    Task<ApiResponse<PagedResult<RoomListDto>>> GetListAsync(long hostId, RoomQueryRequest request);
    Task<ApiResponse<RoomListDto>> GetByIdAsync(long hostId, long id);
    Task<ApiResponse<RoomListDto>> CreateAsync(long hostId, RoomCreateRequest request);
    Task<ApiResponse<RoomListDto>> UpdateAsync(long hostId, long id, RoomUpdateRequest request);
    Task<ApiResponse<bool>> DeleteAsync(long hostId, long id);
    Task<ApiResponse<bool>> BatchDeleteAsync(long hostId, List<long> ids);
}

public class RoomService : IRoomService
{
    private readonly AppDbContext _context;
    private readonly ILogger<RoomService> _logger;

    public RoomService(AppDbContext context, ILogger<RoomService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<RoomListDto>>> GetListAsync(long hostId, RoomQueryRequest request)
    {
        var propertyIds = await _context.Properties
            .Where(p => p.HostId == hostId)
            .Select(p => p.Id)
            .ToListAsync();

        var query = _context.Rooms
            .Include(r => r.Property)
            .Where(r => propertyIds.Contains(r.PropertyId));

        if (request.PropertyId.HasValue)
        {
            query = query.Where(r => r.PropertyId == request.PropertyId.Value);
        }
        if (!string.IsNullOrEmpty(request.RoomType))
        {
            query = query.Where(r => r.RoomType == request.RoomType);
        }
        if (request.Status.HasValue)
        {
            query = query.Where(r => r.Status == request.Status.Value);
        }

        var total = await query.CountAsync();

        var rawItems = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new
            {
                r.Id,
                r.PropertyId,
                r.Name,
                r.RoomType,
                r.Area,
                r.BedType,
                r.Beds,
                r.MaxGuests,
                r.PricePerNight,
                r.Floor,
                r.Status,
                r.Facilities,
                r.Description,
                PropertyName = r.Property != null ? r.Property.Title : null,
                r.CreatedAt,
                r.UpdatedAt
            })
            .ToListAsync();

        var items = rawItems.Select(r => new RoomListDto
        {
            Id = r.Id,
            PropertyId = r.PropertyId,
            Name = r.Name,
            RoomType = r.RoomType,
            Area = r.Area,
            BedType = r.BedType,
            Beds = r.Beds,
            MaxGuests = r.MaxGuests,
            PricePerNight = r.PricePerNight,
            Floor = r.Floor,
            Status = r.Status,
            Facilities = r.Facilities != null ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(r.Facilities) : null,
            Description = r.Description,
            PropertyName = r.PropertyName,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToList();

        return ApiResponse<PagedResult<RoomListDto>>.Ok(new PagedResult<RoomListDto>
        {
            Items = items,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)request.PageSize)
        });
    }

    public async Task<ApiResponse<RoomListDto>> GetByIdAsync(long hostId, long id)
    {
        var propertyIds = await _context.Properties
            .Where(p => p.HostId == hostId)
            .Select(p => p.Id)
            .ToListAsync();

        var room = await _context.Rooms
            .Include(r => r.Property)
            .FirstOrDefaultAsync(r => r.Id == id && propertyIds.Contains(r.PropertyId));

        if (room == null)
        {
            return ApiResponse<RoomListDto>.Fail("房间不存在");
        }

        return ApiResponse<RoomListDto>.Ok(MapToDto(room));
    }

    public async Task<ApiResponse<RoomListDto>> CreateAsync(long hostId, RoomCreateRequest request)
    {
        var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == request.PropertyId && p.HostId == hostId);
        if (property == null)
        {
            return ApiResponse<RoomListDto>.Fail("房源不存在");
        }

        var room = new Room
        {
            PropertyId = request.PropertyId,
            Name = request.Name,
            RoomType = request.RoomType,
            Area = request.Area,
            BedType = request.BedType,
            Beds = request.Beds,
            MaxGuests = request.MaxGuests,
            PricePerNight = request.PricePerNight,
            Floor = request.Floor,
            Status = 1,
            Facilities = request.Facilities != null ? System.Text.Json.JsonSerializer.Serialize(request.Facilities) : null,
            Description = request.Description,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        _logger.LogInformation("房东 {HostId} 创建房间 {RoomId}", hostId, room.Id);

        return await GetByIdAsync(hostId, room.Id);
    }

    public async Task<ApiResponse<RoomListDto>> UpdateAsync(long hostId, long id, RoomUpdateRequest request)
    {
        var propertyIds = await _context.Properties
            .Where(p => p.HostId == hostId)
            .Select(p => p.Id)
            .ToListAsync();

        var room = await _context.Rooms
            .Include(r => r.Property)
            .FirstOrDefaultAsync(r => r.Id == id && propertyIds.Contains(r.PropertyId));

        if (room == null)
        {
            return ApiResponse<RoomListDto>.Fail("房间不存在");
        }

        if (request.Name != null) room.Name = request.Name;
        if (request.RoomType != null) room.RoomType = request.RoomType;
        if (request.Area.HasValue) room.Area = request.Area.Value;
        if (request.BedType != null) room.BedType = request.BedType;
        if (request.Beds.HasValue) room.Beds = request.Beds.Value;
        if (request.MaxGuests.HasValue) room.MaxGuests = request.MaxGuests.Value;
        if (request.PricePerNight.HasValue) room.PricePerNight = request.PricePerNight.Value;
        if (request.Floor.HasValue) room.Floor = request.Floor.Value;
        if (request.Status.HasValue) room.Status = request.Status.Value;
        if (request.Facilities != null) room.Facilities = System.Text.Json.JsonSerializer.Serialize(request.Facilities);
        if (request.Description != null) room.Description = request.Description;

        room.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        _logger.LogInformation("房东 {HostId} 更新房间 {RoomId}", hostId, id);

        return ApiResponse<RoomListDto>.Ok(MapToDto(room));
    }

    public async Task<ApiResponse<bool>> DeleteAsync(long hostId, long id)
    {
        var propertyIds = await _context.Properties
            .Where(p => p.HostId == hostId)
            .Select(p => p.Id)
            .ToListAsync();

        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == id && propertyIds.Contains(r.PropertyId));
        if (room == null)
        {
            return ApiResponse<bool>.Fail("房间不存在");
        }

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();

        _logger.LogInformation("房东 {HostId} 删除房间 {RoomId}", hostId, id);

        return ApiResponse<bool>.Ok(true, "删除成功");
    }

    public async Task<ApiResponse<bool>> BatchDeleteAsync(long hostId, List<long> ids)
    {
        var propertyIds = await _context.Properties
            .Where(p => p.HostId == hostId)
            .Select(p => p.Id)
            .ToListAsync();

        var rooms = await _context.Rooms
            .Where(r => propertyIds.Contains(r.PropertyId) && ids.Contains(r.Id))
            .ToListAsync();

        if (rooms.Count == 0)
        {
            return ApiResponse<bool>.Fail("未找到可删除的房间");
        }

        _context.Rooms.RemoveRange(rooms);
        await _context.SaveChangesAsync();

        _logger.LogInformation("房东 {HostId} 批量删除 {Count} 个房间", hostId, rooms.Count);

        return ApiResponse<bool>.Ok(true, $"成功删除 {rooms.Count} 个房间");
    }

    private static RoomListDto MapToDto(Room r)
    {
        return new RoomListDto
        {
            Id = r.Id,
            PropertyId = r.PropertyId,
            Name = r.Name,
            RoomType = r.RoomType,
            Area = r.Area,
            BedType = r.BedType,
            Beds = r.Beds,
            MaxGuests = r.MaxGuests,
            PricePerNight = r.PricePerNight,
            Floor = r.Floor,
            Status = r.Status,
            Facilities = r.Facilities != null ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(r.Facilities) : null,
            Description = r.Description,
            PropertyName = r.Property?.Title,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        };
    }
}
