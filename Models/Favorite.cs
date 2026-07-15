namespace qisu_server.Models;

public class Favorite
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long PropertyId { get; set; }
    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
    public Property? Property { get; set; }
}
