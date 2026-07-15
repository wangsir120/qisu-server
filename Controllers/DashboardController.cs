using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models.DTOs;

namespace qisu_server.Controllers;

/// <summary>
/// 仪表盘数据控制器
/// </summary>
[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(AppDbContext context, ILogger<DashboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取仪表盘统计数据
    /// </summary>
    [HttpGet("stats")]
    public async Task<ApiResponse<object>> GetStats()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return ApiResponse<object>.Fail("用户未登录");

            var isAdmin = await IsAdmin(userId.Value);

            return isAdmin
                ? ApiResponse<object>.Ok(await GetAdminStats())
                : ApiResponse<object>.Ok(await GetLandlordStats(userId.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取仪表盘统计数据失败");
            return ApiResponse<object>.Fail("获取统计数据失败");
        }
    }

    /// <summary>
    /// 获取趋势数据（入住率/订单数/营收）
    /// </summary>
    [HttpGet("trend")]
    public async Task<ApiResponse<List<TrendDataDto>>> GetTrend([FromQuery] string type = "occupancy", [FromQuery] string range = "month")
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return ApiResponse<List<TrendDataDto>>.Fail("用户未登录");

            var (startDate, endDate, labels) = GetDateRange(range);
            var isAdmin = await IsAdmin(userId.Value);
            var hostId = isAdmin ? (long?)null : await GetHostId(userId.Value);

            var result = new List<TrendDataDto>();

            if (type == "orders")
            {
                var ordersQuery = _context.Orders.AsQueryable();
                if (!isAdmin && hostId.HasValue) ordersQuery = ordersQuery.Where(o => o.HostId == hostId.Value);
                ordersQuery = ordersQuery.Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate);

                foreach (var label in labels)
                {
                    var (dayStart, dayEnd) = GetDayRange(label, startDate);
                    var count = await ordersQuery.CountAsync(o => o.CreatedAt >= dayStart && o.CreatedAt < dayEnd);
                    result.Add(new TrendDataDto { Label = label, Value = count });
                }
            }
            else if (type == "checkin")
            {
                var checkinQuery = _context.Orders.Where(o => o.Status != "cancelled");
                if (!isAdmin && hostId.HasValue) checkinQuery = checkinQuery.Where(o => o.HostId == hostId.Value);
                checkinQuery = checkinQuery.Where(o => o.CheckInDate >= startDate && o.CheckInDate <= endDate);

                foreach (var label in labels)
                {
                    var (dayStart, dayEnd) = GetDayRange(label, startDate);
                    var count = await checkinQuery.CountAsync(o => o.CheckInDate >= dayStart && o.CheckInDate < dayEnd);
                    result.Add(new TrendDataDto { Label = label, Value = count });
                }
            }
            else if (type == "revenue")
            {
                var revenueQuery = _context.Orders.Where(o => o.Status != "cancelled");
                if (!isAdmin && hostId.HasValue) revenueQuery = revenueQuery.Where(o => o.HostId == hostId.Value);
                revenueQuery = revenueQuery.Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate);

                foreach (var label in labels)
                {
                    var (dayStart, dayEnd) = GetDayRange(label, startDate);
                    var total = await revenueQuery
                        .Where(o => o.CreatedAt >= dayStart && o.CreatedAt < dayEnd)
                        .SumAsync(o => o.TotalPrice);
                    result.Add(new TrendDataDto { Label = label, Value = total });
                }
            }
            else
            {
                foreach (var label in labels)
                {
                    result.Add(new TrendDataDto { Label = label, Value = 0 });
                }
            }

            return ApiResponse<List<TrendDataDto>>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取趋势数据失败");
            return ApiResponse<List<TrendDataDto>>.Fail("获取趋势数据失败");
        }
    }

    /// <summary>
    /// 获取客户来源分布
    /// </summary>
    [HttpGet("source-distribution")]
    public async Task<ApiResponse<List<SourceDistributionDto>>> GetSourceDistribution([FromQuery] string range = "month")
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return ApiResponse<List<SourceDistributionDto>>.Fail("用户未登录");

            var (startDate, _, _) = GetDateRange(range);
            var isAdmin = await IsAdmin(userId.Value);
            var hostId = isAdmin ? (long?)null : await GetHostId(userId.Value);

            var query = _context.Orders.Where(o => o.CreatedAt >= startDate);
            if (!isAdmin && hostId.HasValue) query = query.Where(o => o.HostId == hostId.Value);

            var totalOrders = await query.CountAsync();

            var result = new List<SourceDistributionDto>
            {
                new() { Name = "直接预订", Value = totalOrders, Color = "#e8943a" },
                new() { Name = "搜索发现", Value = 0, Color = "#1890ff" },
                new() { Name = "分享链接", Value = 0, Color = "#52c41a" },
                new() { Name = "其他渠道", Value = 0, Color = "#722ed1" }
            };

            return ApiResponse<List<SourceDistributionDto>>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取来源分布失败");
            return ApiResponse<List<SourceDistributionDto>>.Fail("获取来源分布失败");
        }
    }

    /// <summary>
    /// 获取营收趋势数据
    /// </summary>
    [HttpGet("revenue-trend")]
    public async Task<ApiResponse<List<RevenueTrendDto>>> GetRevenueTrend([FromQuery] string range = "month")
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return ApiResponse<List<RevenueTrendDto>>.Fail("用户未登录");

            var (startDate, endDate, labels) = GetDateRange(range);
            var isAdmin = await IsAdmin(userId.Value);
            var hostId = isAdmin ? (long?)null : await GetHostId(userId.Value);

            var query = _context.Orders.Where(o => o.Status != "cancelled" && o.CreatedAt >= startDate && o.CreatedAt <= endDate);
            if (!isAdmin && hostId.HasValue) query = query.Where(o => o.HostId == hostId.Value);

            var result = new List<RevenueTrendDto>();

            foreach (var label in labels)
            {
                var (dayStart, dayEnd) = GetDayRange(label, startDate);
                var revenue = await query
                    .Where(o => o.CreatedAt >= dayStart && o.CreatedAt < dayEnd)
                    .SumAsync(o => o.TotalPrice);
                var count = await query
                    .Where(o => o.CreatedAt >= dayStart && o.CreatedAt < dayEnd)
                    .CountAsync();

                result.Add(new RevenueTrendDto
                {
                    Label = label,
                    Revenue = revenue,
                    OrderCount = count
                });
            }

            return ApiResponse<List<RevenueTrendDto>>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取营收趋势失败");
            return ApiResponse<List<RevenueTrendDto>>.Fail("获取营收趋势失败");
        }
    }

    /// <summary>
    /// 获取月度对比数据
    /// </summary>
    [HttpGet("monthly-compare")]
    public async Task<ApiResponse<List<MonthlyCompareDto>>> GetMonthlyCompare([FromQuery] int months = 6)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return ApiResponse<List<MonthlyCompareDto>>.Fail("用户未登录");

            var isAdmin = await IsAdmin(userId.Value);
            var hostId = isAdmin ? (long?)null : await GetHostId(userId.Value);

            var result = new List<MonthlyCompareDto>();
            var today = DateTime.Today;

            for (int i = 0; i < months; i++)
            {
                var currentMonthStart = new DateTime(today.Year, today.Month, 1).AddMonths(-i);
                var currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);
                var lastMonthStart = currentMonthStart.AddMonths(-1);
                var lastMonthEnd = currentMonthStart.AddDays(-1);

                var baseQuery = _context.Orders.Where(o => o.Status != "cancelled");
                if (!isAdmin && hostId.HasValue) baseQuery = baseQuery.Where(o => o.HostId == hostId.Value);

                var currentRevenue = await baseQuery
                    .Where(o => o.CreatedAt >= currentMonthStart && o.CreatedAt <= currentMonthEnd)
                    .SumAsync(o => o.TotalPrice);
                var lastRevenue = await baseQuery
                    .Where(o => o.CreatedAt >= lastMonthStart && o.CreatedAt <= lastMonthEnd)
                    .SumAsync(o => o.TotalPrice);

                var maxRevenue = Math.Max(Math.Max(currentRevenue, lastRevenue), 1);

                result.Add(new MonthlyCompareDto
                {
                    Month = $"{currentMonthStart:MM月}",
                    Current = currentRevenue,
                    Last = lastRevenue,
                    CurrentPercent = maxRevenue > 0 ? (int)(currentRevenue / maxRevenue * 100) : 0,
                    LastPercent = maxRevenue > 0 ? (int)(lastRevenue / maxRevenue * 100) : 0
                });
            }

            result.Reverse();

            return ApiResponse<List<MonthlyCompareDto>>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取月度对比失败");
            return ApiResponse<List<MonthlyCompareDto>>.Fail("获取月度对比失败");
        }
    }

    /// <summary>
    /// 获取房源营收排行
    /// </summary>
    [HttpGet("ranking")]
    public async Task<ApiResponse<List<PropertyRankingDto>>> GetRanking([FromQuery] string type = "revenue", [FromQuery] string range = "month")
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return ApiResponse<List<PropertyRankingDto>>.Fail("用户未登录");

            var (startDate, _, _) = GetDateRange(range);
            var isAdmin = await IsAdmin(userId.Value);
            var hostId = isAdmin ? (long?)null : await GetHostId(userId.Value);

            var query = _context.Orders
                .Where(o => o.Status != "cancelled" && o.CreatedAt >= startDate)
                .GroupBy(o => o.PropertyId)
                .Select(g => new PropertyRankingDto
                {
                    Id = g.Key,
                    Name = g.FirstOrDefault().Property != null ? g.FirstOrDefault().Property!.Title : "",
                    Revenue = g.Sum(o => o.TotalPrice),
                    Orders = g.Count()
                })
                .OrderByDescending(g => g.Revenue)
                .Take(10);

            if (!isAdmin && hostId.HasValue)
            {
                query = query.Where(g => _context.Properties.Any(p => p.Id == g.Id && p.HostId == hostId.Value));
            }

            var list = await query.ToListAsync();

            var maxRevenue = list.FirstOrDefault()?.Revenue ?? 1;
            foreach (var item in list)
            {
                item.Revenue = Math.Round(item.Revenue, 2);
            }

            return ApiResponse<List<PropertyRankingDto>>.Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取排行数据失败");
            return ApiResponse<List<PropertyRankingDto>>.Fail("获取排行数据失败");
        }
    }

    /// <summary>
    /// 获取最近注册用户列表（仅管理员）
    /// </summary>
    [HttpGet("recent-users")]
    public async Task<ApiResponse<List<RecentUserDto>>> GetRecentUsers([FromQuery] int count = 5)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return ApiResponse<List<RecentUserDto>>.Fail("用户未登录");

            if (!await IsAdmin(userId.Value)) return ApiResponse<List<RecentUserDto>>.Fail("无权限访问");

            var users = await _context.Users.OrderByDescending(u => u.CreatedAt).Take(count).ToListAsync();

            var result = users.Select(u => new RecentUserDto
            {
                Id = u.Id,
                Username = u.Username,
                Nickname = u.Nickname,
                Phone = u.Phone,
                Avatar = u.Avatar,
                Status = u.Status == 1 ? "active" : "inactive",
                CreatedAt = u.CreatedAt
            }).ToList();

            return ApiResponse<List<RecentUserDto>>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取最近用户失败");
            return ApiResponse<List<RecentUserDto>>.Fail("获取最近用户失败");
        }
    }

    /// <summary>
    /// 获取待审核的房东申请列表（仅管理员）
    /// </summary>
    [HttpGet("pending-applications")]
    public async Task<ApiResponse<List<PendingApplicationDto>>> GetPendingApplications([FromQuery] int count = 5)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return ApiResponse<List<PendingApplicationDto>>.Fail("用户未登录");

            if (!await IsAdmin(userId.Value)) return ApiResponse<List<PendingApplicationDto>>.Fail("无权限访问");

            var applications = await _context.HostApplications
                .Where(h => h.Status == "pending")
                .OrderByDescending(h => h.CreatedAt)
                .Take(count)
                .ToListAsync();

            var result = applications.Select(a => new PendingApplicationDto
            {
                Id = a.Id,
                Name = a.Name,
                Phone = a.Phone,
                PropertyTitle = a.PropertyTitle,
                PropertyType = a.PropertyType,
                Status = a.Status,
                CreatedAt = a.CreatedAt
            }).ToList();

            return ApiResponse<List<PendingApplicationDto>>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取待审核申请失败");
            return ApiResponse<List<PendingApplicationDto>>.Fail("获取待审核申请失败");
        }
    }

    #region Private Methods

    private async Task<object> GetAdminStats()
    {
        var today = DateTime.Today;
        var todayEnd = today.AddDays(1);

        var totalUsers = await _context.Users.CountAsync();
        var totalLandlords = await _context.Hosts.CountAsync();
        var todayApplications = await _context.HostApplications
            .Where(h => h.CreatedAt >= today && h.CreatedAt < todayEnd).CountAsync();
        var totalListings = await _context.Hosts.SumAsync(h => h.TotalListings);
        var pendingApplications = await _context.HostApplications
            .Where(h => h.Status == "pending").CountAsync();
        var unreadMessages = await _context.ChatConversations.SumAsync(c => c.UnreadCount);
        var todayConversations = await _context.ChatConversations
            .Where(c => c.LastMessageTime >= today && c.LastMessageTime < todayEnd).CountAsync();

        var totalOrders = await _context.Orders.CountAsync();
        var pendingOrders = await _context.Orders
            .Where(o => o.Status == "pending").CountAsync();
        var todayCheckIns = await _context.Orders
            .Where(o => o.CheckInDate == today && o.Status != "cancelled").CountAsync();
        var todayCheckOuts = await _context.Orders
            .Where(o => o.CheckOutDate == today && o.Status != "cancelled").CountAsync();
        var staying = await _context.Orders
            .Where(o => o.CheckInDate <= today && o.CheckOutDate > today && o.Status != "cancelled").CountAsync();
        var todayRevenue = await _context.Orders
            .Where(o => o.PaymentTime.HasValue && o.PaymentTime.Value.Date == today && o.Status != "cancelled")
            .SumAsync(o => o.TotalPrice);

        var totalCompletedOrders = await _context.Orders
            .Where(o => o.Status == "completed").CountAsync();

        return new
        {
            TotalUsers = totalUsers,
            TotalLandlords = totalLandlords,
            TodayApplications = todayApplications,
            TotalListings = totalListings,
            PendingApplications = pendingApplications,
            UnreadMessages = unreadMessages,
            TodayConversations = todayConversations,
            TotalOrders = totalOrders,
            PendingOrders = pendingOrders,
            TodayCheckIns = todayCheckIns,
            TodayCheckOuts = todayCheckOuts,
            Staying = staying,
            TodayRevenue = todayRevenue,
            RecentCompletedOrders = totalCompletedOrders
        };
    }

    private async Task<LandlordDashboardStatsDto> GetLandlordStats(long userId)
    {
        var host = await _context.Hosts.FirstOrDefaultAsync(h => h.UserId == userId);
        var hostId = host?.Id ?? 0;
        var today = DateTime.Today;
        var todayEnd = today.AddDays(1);

        var pendingOrders = await _context.Orders
            .Where(o => o.HostId == hostId && o.Status == "pending").CountAsync();

        var todayCheckIns = await _context.Orders
            .Where(o => o.HostId == hostId && o.CheckInDate == today && o.Status != "cancelled").CountAsync();

        var todayCheckOuts = await _context.Orders
            .Where(o => o.HostId == hostId && o.CheckOutDate == today && o.Status != "cancelled").CountAsync();

        var staying = await _context.Orders
            .Where(o => o.HostId == hostId && o.CheckInDate <= today && o.CheckOutDate > today && o.Status != "cancelled").CountAsync();

        var todayRevenue = await _context.Orders
            .Where(o => o.HostId == hostId && o.PaymentTime.HasValue
                && o.PaymentTime.Value.Date == today && o.Status != "cancelled")
            .SumAsync(o => o.TotalPrice);

        var totalCompletedOrders = await _context.Orders
            .Where(o => o.HostId == hostId && o.Status == "completed").CountAsync();

        return new LandlordDashboardStatsDto
        {
            TotalListings = host?.TotalListings ?? 0,
            TotalReviews = host?.TotalReviews ?? 0,
            Rating = host?.Rating ?? 0,
            PendingOrders = pendingOrders,
            TodayCheckIns = todayCheckIns,
            TodayCheckOuts = todayCheckOuts,
            Staying = staying,
            TodayRevenue = todayRevenue,
            RecentCompletedOrders = totalCompletedOrders
        };
    }

    private static (DateTime Start, DateTime End, List<string> Labels) GetDateRange(string range)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        switch (range)
        {
            case "today":
                return (today, tomorrow, new List<string> { today.ToString("MM-dd") });
            case "week":
                var weekStart = today.AddDays(-6);
                var weekLabels = Enumerable.Range(0, 7).Select(i => weekStart.AddDays(i).ToString("MM-dd")).ToList();
                return (weekStart, tomorrow, weekLabels);
            case "month":
                var monthStart = today.AddDays(-29);
                var monthLabels = Enumerable.Range(0, 30).Select(i => monthStart.AddDays(i).ToString("MM-dd")).ToList();
                return (monthStart, tomorrow, monthLabels);
            case "quarter":
                var quarterStart = today.AddDays(-89);
                var quarterLabels = Enumerable.Range(0, 90).Select(i => quarterStart.AddDays(i).ToString("MM-dd")).ToList();
                return (quarterStart, tomorrow, quarterLabels);
            case "year":
                var yStart = today.AddYears(-1).AddDays(1);
                var yLabels = new List<string>();
                var cursor = new DateTime(yStart.Year, yStart.Month, 1);
                while (cursor <= new DateTime(today.Year, today.Month, 1))
                {
                    yLabels.Add(cursor.ToString("yyyy-MM"));
                    cursor = cursor.AddMonths(1);
                }
                return (yStart, tomorrow, yLabels);
            default:
                var defaultStart = today.AddDays(-29);
                var defaultLabels = Enumerable.Range(0, 30).Select(i => defaultStart.AddDays(i).ToString("MM-dd")).ToList();
                return (defaultStart, tomorrow, defaultLabels);
        }
    }

    private static (DateTime Start, DateTime End) GetDayRange(string label, DateTime rangeStart)
    {
        if (label.Contains("-") && label.Length == 5)
        {
            var parts = label.Split('-');
            if (int.TryParse(parts[0], out var month) && int.TryParse(parts[1], out var day))
            {
                var date = new DateTime(rangeStart.Year, month, day);
                return (date, date.AddDays(1));
            }
        }
        if (label.Contains("-") && label.Length == 7)
        {
            var parts = label.Split('-');
            if (int.TryParse(parts[0], out var year) && int.TryParse(parts[1], out var m))
            {
                var start = new DateTime(year, m, 1);
                return (start, start.AddMonths(1));
            }
        }
        return (DateTime.Today, DateTime.Today.AddDays(1));
    }

    private async Task<long?> GetHostId(long userId)
    {
        var host = await _context.Hosts.FirstOrDefaultAsync(h => h.UserId == userId);
        return host?.Id;
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            userIdClaim = User.FindFirst("sub")?.Value;
        return string.IsNullOrEmpty(userIdClaim) ? null : long.TryParse(userIdClaim, out var id) ? id : null;
    }

    private async Task<bool> IsAdmin(long userId) => await _context.Admins.FindAsync(userId) != null;

    #endregion
}
