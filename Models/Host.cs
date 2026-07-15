namespace qisu_server.Models;

public class Host
{
    public long Id { get; set; }
    public long? UserId { get; set; }
    public string? Name { get; set; }
    public string? Avatar { get; set; }
    public string? Phone { get; set; }
    public bool IsSuperhost { get; set; }
    public decimal ResponseRate { get; set; }
    public string? ResponseTime { get; set; }
    public bool Verified { get; set; }
    public int TotalListings { get; set; }
    public int TotalReviews { get; set; }
    public decimal Rating { get; set; }
    public byte Status { get; set; } = 1;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
