namespace qisu_server.Models;

using System.ComponentModel.DataAnnotations.Schema;

public class HotDestination
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Image { get; set; }
    public int PropertyCount { get; set; }
    public int SortOrder { get; set; }
    public bool Status { get; set; }

    public int SearchCount { get; set; }
    public int BookingCount { get; set; }
    public int ViewCount { get; set; }
    public decimal HotScore { get; set; }
    public string? LastUpdatedBy { get; set; }

    [NotMapped]
    public string? Description { get; set; }

    [NotMapped]
    public string? Region { get; set; }

    [NotMapped]
    public decimal Rating { get; set; } = 4.5m;

    [NotMapped]
    public string? BestTime { get; set; }

    [NotMapped]
    public string? TrafficGuide { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
