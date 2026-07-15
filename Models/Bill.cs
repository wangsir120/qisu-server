namespace qisu_server.Models;

public class Bill
{
    public long Id { get; set; }
    public long HostId { get; set; }
    public string Type { get; set; } = "income";
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? OrderNo { get; set; }
    public string? GuestName { get; set; }
    public string? PayMethod { get; set; }
    public string Status { get; set; } = "completed";
    public string? Remark { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Host? Host { get; set; }
}
