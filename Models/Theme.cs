namespace qisu_server.Models;

public class Theme
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public int PropertyCount { get; set; }
    public int SortOrder { get; set; }
    public bool Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<PropertyTheme> PropertyThemes { get; set; } = new();
}
