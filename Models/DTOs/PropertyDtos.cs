using System.ComponentModel.DataAnnotations;

public class PropertyQueryRequest
{
    public string? Title { get; set; }
    public string? PropertyType { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class PropertyCreateRequest
{
    [Required(ErrorMessage = "房源标题不能为空")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }

    public string? PropertyType { get; set; }
    public decimal? Area { get; set; }

    [MaxLength(200)]
    public string? BedType { get; set; }

    public int MaxGuests { get; set; } = 1;

    [Range(1, 50, ErrorMessage = "房间数必须在1-50之间")]
    public int Bedrooms { get; set; } = 1;

    public long? AddressId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PricePerNight { get; set; }

    public List<string>? Facilities { get; set; }
    public List<PropertyImageInput>? Images { get; set; }
    public int Floor { get; set; } = 1;
    public bool IsInstantBook { get; set; } = false;
    public bool IsNew { get; set; } = false;
}

public class PropertyUpdateRequest
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(4000)]
    public string? Description { get; set; }

    public string? PropertyType { get; set; }
    public decimal? Area { get; set; }

    [MaxLength(200)]
    public string? BedType { get; set; }

    public int? MaxGuests { get; set; }

    [Range(1, 50, ErrorMessage = "房间数必须在1-50之间")]
    public int? Bedrooms { get; set; }

    public long? AddressId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? PricePerNight { get; set; }

    public List<string>? Facilities { get; set; }
    public List<PropertyImageInput>? Images { get; set; }
    public int? Floor { get; set; }
    public bool? IsInstantBook { get; set; }
    public bool? IsNew { get; set; }
    public byte? Status { get; set; }
}

public class PropertyListDto
{
    public long Id { get; set; }
    public long HostId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PropertyType { get; set; }
    public decimal? Area { get; set; }
    public string? BedType { get; set; }
    public int MaxGuests { get; set; }
    public int Bedrooms { get; set; }
    public int RoomCount { get; set; }
    public long? AddressId { get; set; }
    public string? AddressName { get; set; }
    public decimal PricePerNight { get; set; }
    public byte Status { get; set; }
    public bool IsInstantBook { get; set; }
    public bool IsNew { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CoverImage { get; set; }
    public List<string>? Facilities { get; set; }
    public List<string>? Images { get; set; }
}

public class PropertyImageInput
{
    public string Url { get; set; } = string.Empty;
    public bool IsCover { get; set; }
}
