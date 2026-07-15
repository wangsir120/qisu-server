namespace qisu_server.Models.DTOs;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }

    public int TotalCount 
    { 
        get => Total;
        set => Total = value;
    }
}
