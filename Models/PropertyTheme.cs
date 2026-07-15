namespace qisu_server.Models;

public class PropertyTheme
{
    public long Id { get; set; }
    public long PropertyId { get; set; }
    public long ThemeId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Property Property { get; set; } = null!;
    public Theme Theme { get; set; } = null!;
}