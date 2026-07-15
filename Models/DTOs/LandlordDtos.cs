using System.ComponentModel.DataAnnotations;

public class LandlordQueryRequest
{
    public string? Keyword { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class LandlordListDto
{
    public long Id { get; set; }
    public long? UserId { get; set; }
    public string? Name { get; set; }
    public string? Avatar { get; set; }
    public string? Phone { get; set; }
    public bool IsSuperhost { get; set; }
    public bool Verified { get; set; }
    public int TotalListings { get; set; }
    public int TotalReviews { get; set; }
    public decimal Rating { get; set; }
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; }
}

public class LandlordDetailDto
{
    public long Id { get; set; }
    public long? UserId { get; set; }
    public string? Name { get; set; }
    public string? Avatar { get; set; }
    public string? Phone { get; set; }
    public bool IsSuperhost { get; set; }
    public bool Verified { get; set; }
    public int TotalListings { get; set; }
    public int TotalReviews { get; set; }
    public decimal Rating { get; set; }
    public decimal ResponseRate { get; set; }
    public string? ResponseTime { get; set; }
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class LandlordStatsDto
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Superhost { get; set; }
    public int NewThisMonth { get; set; }
}
