using System.ComponentModel.DataAnnotations;

public class AddressQueryRequest
{
    public string? Keyword { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class AddressCreateRequest
{
    [Required(ErrorMessage = "联系人姓名不能为空")]
    [MaxLength(100, ErrorMessage = "姓名最长100个字符")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "联系电话不能为空")]
    [MaxLength(20, ErrorMessage = "电话最长20个字符")]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Province { get; set; }

    [MaxLength(50)]
    public string? City { get; set; }

    [MaxLength(50)]
    public string? District { get; set; }

    [Required(ErrorMessage = "地址不能为空")]
    [MaxLength(500, ErrorMessage = "地址最长500个字符")]
    public string Detail { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? FullAddress { get; set; }

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    [MaxLength(50)]
    public string? PoiId { get; set; }

    [MaxLength(200)]
    public string? PoiName { get; set; }

    public bool IsDefault { get; set; }

    [MaxLength(500)]
    public string? Remark { get; set; }
}

public class AddressUpdateRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(50)]
    public string? Province { get; set; }

    [MaxLength(50)]
    public string? City { get; set; }

    [MaxLength(50)]
    public string? District { get; set; }

    [MaxLength(500)]
    public string? Detail { get; set; }

    [MaxLength(500)]
    public string? FullAddress { get; set; }

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    [MaxLength(50)]
    public string? PoiId { get; set; }

    [MaxLength(200)]
    public string? PoiName { get; set; }

    public bool? IsDefault { get; set; }

    [MaxLength(500)]
    public string? Remark { get; set; }
}

public class AddressListDto
{
    public long Id { get; set; }
    public long HostId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string? FullAddress { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? PoiId { get; set; }
    public string? PoiName { get; set; }
    public bool IsDefault { get; set; }
    public string? Remark { get; set; }
    public byte Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
