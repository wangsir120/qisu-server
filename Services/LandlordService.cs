using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models;
using qisu_server.Models.DTOs;

namespace qisu_server.Services;

public interface ILandlordService
{
    Task<ApiResponse<LandlordStatsDto>> GetStatsAsync();
    Task<ApiResponse<PagedResult<LandlordListDto>>> GetListAsync(LandlordQueryRequest request);
    Task<ApiResponse<LandlordDetailDto>> GetByIdAsync(long id);
    Task<ApiResponse<bool>> ToggleStatusAsync(long id);
}

public class LandlordService : ILandlordService
{
    private readonly AppDbContext _context;
    private readonly ILogger<LandlordService> _logger;

    public LandlordService(AppDbContext context, ILogger<LandlordService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<LandlordStatsDto>> GetStatsAsync()
    {
        var total = await _context.Hosts.CountAsync();
        var active = await _context.Hosts.CountAsync(h => h.Status == (byte)1);
        var superhost = await _context.Hosts.CountAsync(h => h.IsSuperhost == true);
        
        var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var newThisMonth = await _context.Hosts.CountAsync(h => h.CreatedAt >= startOfMonth);

        var stats = new LandlordStatsDto
        {
            Total = total,
            Active = active,
            Superhost = superhost,
            NewThisMonth = newThisMonth
        };

        return ApiResponse<LandlordStatsDto>.Ok(stats);
    }

    public async Task<ApiResponse<PagedResult<LandlordListDto>>> GetListAsync(LandlordQueryRequest request)
    {
        var query = from h in _context.Hosts
                    join u in _context.Users on h.UserId equals u.Id into userJoin
                    from u in userJoin.DefaultIfEmpty()
                    select new { Host = h, User = u };

        if (!string.IsNullOrEmpty(request.Keyword))
        {
            query = query.Where(x => 
                (x.Host.Name != null && x.Host.Name.Contains(request.Keyword)) ||
                (x.Host.Phone != null && x.Host.Phone.Contains(request.Keyword)));
        }

        var total = await query.CountAsync();

        var landlords = await query
            .OrderByDescending(x => x.Host.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new LandlordListDto
            {
                Id = x.Host.Id,
                UserId = x.Host.UserId,
                Name = x.Host.Name ?? (x.User != null ? x.User.Nickname : null),
                Avatar = x.Host.Avatar ?? (x.User != null ? x.User.Avatar : null),
                Phone = x.Host.Phone ?? (x.User != null ? x.User.Phone : null),
                IsSuperhost = x.Host.IsSuperhost,
                Verified = x.Host.Verified,
                TotalListings = x.Host.TotalListings,
                TotalReviews = x.Host.TotalReviews,
                Rating = x.Host.Rating,
                Status = x.Host.Status == (byte)1 ? "active" : "inactive",
                CreatedAt = x.Host.CreatedAt
            })
            .ToListAsync();

        var result = new PagedResult<LandlordListDto>
        {
            Items = landlords,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize
        };

        return ApiResponse<PagedResult<LandlordListDto>>.Ok(result);
    }

    public async Task<ApiResponse<LandlordDetailDto>> GetByIdAsync(long id)
    {
        var host = await _context.Hosts.FindAsync(id);
        if (host == null)
        {
            return ApiResponse<LandlordDetailDto>.Fail("房东不存在");
        }

        User? user = null;
        if (host.UserId.HasValue)
        {
            user = await _context.Users.FindAsync(host.UserId.Value);
        }

        var detail = new LandlordDetailDto
        {
            Id = host.Id,
            UserId = host.UserId,
            Name = host.Name ?? user?.Nickname,
            Avatar = host.Avatar ?? user?.Avatar,
            Phone = host.Phone ?? user?.Phone,
            IsSuperhost = host.IsSuperhost,
            Verified = host.Verified,
            TotalListings = host.TotalListings,
            TotalReviews = host.TotalReviews,
            Rating = host.Rating,
            ResponseRate = host.ResponseRate,
            ResponseTime = host.ResponseTime,
            Status = host.Status == (byte)1 ? "active" : "inactive",
            CreatedAt = host.CreatedAt,
            OrderCount = 0,
            TotalRevenue = 0
        };

        return ApiResponse<LandlordDetailDto>.Ok(detail);
    }

    public async Task<ApiResponse<bool>> ToggleStatusAsync(long id)
    {
        var host = await _context.Hosts.FindAsync(id);
        if (host == null)
        {
            return ApiResponse<bool>.Fail("房东不存在");
        }

        host.Status = (byte)(host.Status == (byte)1 ? 0 : 1);
        host.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "操作成功");
    }
}
