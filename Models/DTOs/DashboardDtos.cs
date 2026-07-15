namespace qisu_server.Models.DTOs;

public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalLandlords { get; set; }
    public int TodayApplications { get; set; }
    public int TotalListings { get; set; }
    public int PendingApplications { get; set; }
    public int UnreadMessages { get; set; }
    public int TodayConversations { get; set; }
}

public class LandlordDashboardStatsDto
{
    public int TotalListings { get; set; }
    public int TotalReviews { get; set; }
    public decimal Rating { get; set; }
    public int PendingOrders { get; set; }
    public int TodayCheckIns { get; set; }
    public int TodayCheckOuts { get; set; }
    public int Staying { get; set; }
    public decimal TodayRevenue { get; set; }
    public int RecentCompletedOrders { get; set; }
}

public class TrendDataDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class SourceDistributionDto
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
    public string Color { get; set; } = "#e8943a";
}

public class RevenueTrendDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class MonthlyCompareDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Current { get; set; }
    public decimal Last { get; set; }
    public int CurrentPercent { get; set; }
    public int LastPercent { get; set; }
}

public class PropertyRankingDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
}

public class RecentUserDto
{
    public long Id { get; set; }
    public string? Username { get; set; }
    public string? Nickname { get; set; }
    public string? Phone { get; set; }
    public string? Avatar { get; set; }
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; }
}

public class PendingApplicationDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? PropertyTitle { get; set; }
    public string? PropertyType { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }
}

public class NotificationDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string Type { get; set; } = "info";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
