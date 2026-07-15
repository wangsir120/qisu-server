namespace qisu_server.Models;

public class Address
{
    public long Id { get; set; }
    public long HostId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string? FullAddress { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? PoiId { get; set; }
    public string? PoiName { get; set; }
    public bool IsDefault { get; set; }
    public string? Remark { get; set; }
    public byte Status { get; set; } = 1;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Host? Host { get; set; }
}
