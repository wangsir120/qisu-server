namespace qisu_server.Models;

public class Property
{
    public long Id { get; set; }
    public long HostId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PropertyType { get; set; }
    public decimal? Area { get; set; }
    public string? BedType { get; set; }
    public int Bedrooms { get; set; }
    public int Beds { get; set; }
    public int Bathrooms { get; set; }
    public int MaxGuests { get; set; }
    public decimal PricePerNight { get; set; }
    public decimal? CleaningFee { get; set; }
    public decimal? ServiceFeeRate { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public int ViewCount { get; set; }
    public int FavoriteCount { get; set; }
    public bool IsInstantBook { get; set; }
    public bool IsNew { get; set; }
    public byte Status { get; set; }
    public int? RoomCount { get; set; }
    public string? Facilities { get; set; }
    public long? AddressId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Host? Host { get; set; }
    public Address? PropertyAddress { get; set; }
    public List<PropertyImage> Images { get; set; } = new();
    public List<PropertyTheme> PropertyThemes { get; set; } = new();
}
