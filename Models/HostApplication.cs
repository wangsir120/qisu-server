namespace qisu_server.Models;

public class HostApplication
{
    public long Id { get; set; }
    public long? UserId { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? IdCard { get; set; }
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
    public string Status { get; set; } = "pending";
    public string? AuditRemark { get; set; }
    public long? AuditorId { get; set; }
    public DateTime? AuditedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
