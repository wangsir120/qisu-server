using System.ComponentModel.DataAnnotations;

public class RoomQueryRequest
{
    public long? PropertyId { get; set; }
    public string? RoomType { get; set; }
    public byte? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class RoomCreateRequest
{
    [Required(ErrorMessage = "房源ID不能为空")]
    public long PropertyId { get; set; }

    [Required(ErrorMessage = "房间名称不能为空")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? RoomType { get; set; }

    public decimal? Area { get; set; }

    [MaxLength(200)]
    public string? BedType { get; set; }

    public int Beds { get; set; } = 1;

    public int MaxGuests { get; set; } = 2;

    [Range(0, double.MaxValue)]
    public decimal PricePerNight { get; set; }

    public int Floor { get; set; } = 1;

    public List<string>? Facilities { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }
}

public class RoomUpdateRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(50)]
    public string? RoomType { get; set; }

    public decimal? Area { get; set; }

    [MaxLength(200)]
    public string? BedType { get; set; }

    public int? Beds { get; set; }

    public int? MaxGuests { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? PricePerNight { get; set; }

    public int? Floor { get; set; }

    public byte? Status { get; set; }

    public List<string>? Facilities { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }
}

public class RoomListDto
{
    public long Id { get; set; }
    public long PropertyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RoomType { get; set; }
    public decimal? Area { get; set; }
    public string? BedType { get; set; }
    public int Beds { get; set; }
    public int MaxGuests { get; set; }
    public decimal PricePerNight { get; set; }
    public int Floor { get; set; }
    public byte Status { get; set; }
    public List<string>? Facilities { get; set; }
    public string? Description { get; set; }
    public string? PropertyName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
