namespace qisu_server.Models;

public class PropertyImage
{
    public long Id { get; set; }
    public long PropertyId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsCover { get; set; }
    public DateTime CreatedAt { get; set; }

    public Property? Property { get; set; }
}
