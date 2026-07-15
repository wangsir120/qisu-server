namespace qisu_server.Models.DTOs;

public class BillQueryDto
{
    public string? Type { get; set; }
    public string? Category { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class BillCreateDto
{
    public string Type { get; set; } = "income";
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? PayMethod { get; set; }
    public string? Remark { get; set; }
}
