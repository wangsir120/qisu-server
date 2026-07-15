namespace qisu_server.Models;

public class Room
{
    public long Id { get; set; }
    public long PropertyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RoomType { get; set; }
    public decimal? Area { get; set; }
    public string? BedType { get; set; }
    public int Beds { get; set; } = 1;
    public int MaxGuests { get; set; } = 2;
    public decimal PricePerNight { get; set; }
    public int Floor { get; set; }
    public byte Status { get; set; } = 1;
    public string? Facilities { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Property? Property { get; set; }
}
