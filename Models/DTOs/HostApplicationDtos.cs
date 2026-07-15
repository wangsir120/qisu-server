using System.ComponentModel.DataAnnotations;

public class HostApplicationQueryRequest
{
    public string? Status { get; set; }
    public string? Keyword { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class HostApplicationListDto
{
    public long Id { get; set; }
    public long? UserId { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? IdCard { get; set; }
    public string Status { get; set; } = "pending";
    public string? AuditRemark { get; set; }
    public DateTime? AuditedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class HostApplicationDetailDto : HostApplicationListDto
{
    public string? Email { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Address { get; set; }
    public string? PropertyType { get; set; }
    public int? RoomCount { get; set; }
    public int? BedCount { get; set; }
    public int? GuestCount { get; set; }
    public string? PropertyTitle { get; set; }
    public string? PropertyDesc { get; set; }
    public string? Amenities { get; set; }
    public string? Images { get; set; }
    public long? AuditorId { get; set; }
    public string? AuditorName { get; set; }
}

public class HostApplicationAuditRequest
{
    [Required(ErrorMessage = "审核结果不能为空")]
    public string Status { get; set; } = string.Empty;
    
    public string? AuditRemark { get; set; }
}

public class HostApplicationStatsDto
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
}
