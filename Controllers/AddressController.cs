using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models.DTOs;

namespace qisu_server.Controllers;

/// <summary>
/// 地址管理控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AddressController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AddressController> _logger;

    public AddressController(AppDbContext context, ILogger<AddressController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取地址列表
    /// </summary>
    [HttpGet("list")]
    public async Task<ApiResponse<PagedResult<AddressListDto>>> GetList([FromQuery] AddressQueryRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<PagedResult<AddressListDto>>.Fail("用户未登录");
        }

        var host = await _context.Hosts.FirstOrDefaultAsync(h => h.UserId == userId.Value);
        if (host == null)
        {
            return ApiResponse<PagedResult<AddressListDto>>.Fail("房东信息不存在");
        }

        var query = _context.Addresses.Where(a => a.HostId == host.Id);

        if (!string.IsNullOrEmpty(request.Keyword))
        {
            query = query.Where(a => a.Name.Contains(request.Keyword) || a.Detail.Contains(request.Keyword) || (a.City != null && a.City.Contains(request.Keyword)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AddressListDto
            {
                Id = a.Id,
                HostId = a.HostId,
                Name = a.Name,
                Phone = a.Phone,
                Province = a.Province,
                City = a.City,
                District = a.District,
                Detail = a.Detail,
                FullAddress = a.FullAddress,
                Latitude = a.Latitude,
                Longitude = a.Longitude,
                PoiId = a.PoiId,
                PoiName = a.PoiName,
                IsDefault = a.IsDefault,
                Remark = a.Remark,
                Status = a.Status,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse<PagedResult<AddressListDto>>.Ok(new PagedResult<AddressListDto>
        {
            Items = items,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)request.PageSize)
        });
    }

    /// <summary>
    /// 获取所有地址（不分页）
    /// </summary>
    [HttpGet("all")]
    public async Task<ApiResponse<List<AddressListDto>>> GetAll()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<List<AddressListDto>>.Fail("用户未登录");
        }

        var host = await _context.Hosts.FirstOrDefaultAsync(h => h.UserId == userId.Value);
        if (host == null)
        {
            return ApiResponse<List<AddressListDto>>.Fail("房东信息不存在");
        }

        var items = await _context.Addresses
            .Where(a => a.HostId == host.Id && a.Status == 1)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .Select(a => new AddressListDto
            {
                Id = a.Id,
                HostId = a.HostId,
                Name = a.Name,
                Phone = a.Phone,
                Province = a.Province,
                City = a.City,
                District = a.District,
                Detail = a.Detail,
                FullAddress = a.FullAddress,
                Latitude = a.Latitude,
                Longitude = a.Longitude,
                PoiId = a.PoiId,
                PoiName = a.PoiName,
                IsDefault = a.IsDefault,
                Remark = a.Remark,
                Status = a.Status,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse<List<AddressListDto>>.Ok(items);
    }

    /// <summary>
    /// 获取地址详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ApiResponse<AddressListDto>> GetById(long id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<AddressListDto>.Fail("用户未登录");
        }

        var host = await _context.Hosts.FirstOrDefaultAsync(h => h.UserId == userId.Value);
        if (host == null)
        {
            return ApiResponse<AddressListDto>.Fail("房东信息不存在");
        }

        var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.HostId == host.Id);
        if (address == null)
        {
            return ApiResponse<AddressListDto>.Fail("地址不存在");
        }

        var dto = new AddressListDto
        {
            Id = address.Id,
            HostId = address.HostId,
            Name = address.Name,
            Phone = address.Phone,
            Province = address.Province,
            City = address.City,
            District = address.District,
            Detail = address.Detail,
            FullAddress = address.FullAddress,
            Latitude = address.Latitude,
            Longitude = address.Longitude,
            PoiId = address.PoiId,
            PoiName = address.PoiName,
            IsDefault = address.IsDefault,
            Remark = address.Remark,
            Status = address.Status,
            CreatedAt = address.CreatedAt,
            UpdatedAt = address.UpdatedAt
        };

        return ApiResponse<AddressListDto>.Ok(dto);
    }

    /// <summary>
    /// 创建地址
    /// </summary>
    [HttpPost]
    public async Task<ApiResponse<AddressListDto>> Create([FromBody] AddressCreateRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<AddressListDto>.Fail("用户未登录");
        }

        var host = await _context.Hosts.FirstOrDefaultAsync(h => h.UserId == userId.Value);
        if (host == null)
        {
            return ApiResponse<AddressListDto>.Fail("房东信息不存在");
        }

        var address = new Models.Address
        {
            HostId = host.Id,
            Name = request.Name,
            Phone = request.Phone,
            Province = request.Province,
            City = request.City,
            District = request.District,
            Detail = request.Detail,
            FullAddress = request.FullAddress,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            PoiId = request.PoiId,
            PoiName = request.PoiName,
            IsDefault = request.IsDefault,
            Remark = request.Remark,
            Status = 1,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        if (request.IsDefault)
        {
            var existingDefaults = await _context.Addresses.Where(a => a.HostId == host.Id && a.IsDefault).ToListAsync();
            foreach (var ad in existingDefaults)
            {
                ad.IsDefault = false;
            }
        }

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        _logger.LogInformation("房东 {HostId} 创建地址 {AddressId}", host.Id, address.Id);

        return await GetById(address.Id);
    }

    /// <summary>
    /// 更新地址
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ApiResponse<AddressListDto>> Update(long id, [FromBody] AddressUpdateRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<AddressListDto>.Fail("用户未登录");
        }

        var host = await _context.Hosts.FirstOrDefaultAsync(h => h.UserId == userId.Value);
        if (host == null)
        {
            return ApiResponse<AddressListDto>.Fail("房东信息不存在");
        }

        var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.HostId == host.Id);
        if (address == null)
        {
            return ApiResponse<AddressListDto>.Fail("地址不存在");
        }

        if (request.Name != null) address.Name = request.Name;
        if (request.Phone != null) address.Phone = request.Phone;
        if (request.Province != null) address.Province = request.Province;
        if (request.City != null) address.City = request.City;
        if (request.District != null) address.District = request.District;
        if (request.Detail != null) address.Detail = request.Detail;
        if (request.FullAddress != null) address.FullAddress = request.FullAddress;
        if (request.Latitude != null) address.Latitude = request.Latitude;
        if (request.Longitude != null) address.Longitude = request.Longitude;
        if (request.PoiId != null) address.PoiId = request.PoiId;
        if (request.PoiName != null) address.PoiName = request.PoiName;
        if (request.Remark != null) address.Remark = request.Remark;

        _logger.LogInformation("Update地址 IsDefault={IsDefault}", request.IsDefault);

        if (request.IsDefault.HasValue)
        {
            if (request.IsDefault.Value)
            {
                var existingDefaults = await _context.Addresses
                    .Where(a => a.HostId == host.Id && a.IsDefault && a.Id != id).ToListAsync();
                foreach (var ad in existingDefaults) ad.IsDefault = false;
                address.IsDefault = true;
            }
            else
            {
                address.IsDefault = false;
            }
        }

        address.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        _logger.LogInformation("房东 {HostId} 更新地址 {AddressId}", host.Id, id);

        return await GetById(id);
    }

    /// <summary>
    /// 删除地址
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ApiResponse<bool>> Delete(long id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<bool>.Fail("用户未登录");
        }

        var host = await _context.Hosts.FirstOrDefaultAsync(h => h.UserId == userId.Value);
        if (host == null)
        {
            return ApiResponse<bool>.Fail("房东信息不存在");
        }

        var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.HostId == host.Id);
        if (address == null)
        {
            return ApiResponse<bool>.Fail("地址不存在");
        }

        _context.Addresses.Remove(address);
        await _context.SaveChangesAsync();

        _logger.LogInformation("房东 {HostId} 删除地址 {AddressId}", host.Id, id);

        return ApiResponse<bool>.Ok(true, "删除成功");
    }

    /// <summary>
    /// 设为默认地址
    /// </summary>
    [HttpPost("{id}/set-default")]
    public async Task<ApiResponse<bool>> SetDefault(long id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<bool>.Fail("用户未登录");
        }

        var host = await _context.Hosts.FirstOrDefaultAsync(h => h.UserId == userId.Value);
        if (host == null)
        {
            return ApiResponse<bool>.Fail("房东信息不存在");
        }

        var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.HostId == host.Id);
        if (address == null)
        {
            return ApiResponse<bool>.Fail("地址不存在");
        }

        var existingDefaults = await _context.Addresses.Where(a => a.HostId == host.Id && a.IsDefault).ToListAsync();
        foreach (var ad in existingDefaults)
        {
            ad.IsDefault = false;
        }

        address.IsDefault = true;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "设置成功");
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
            userIdClaim = User.FindFirst("sub")?.Value;
        }
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return null;
        }
        return long.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
