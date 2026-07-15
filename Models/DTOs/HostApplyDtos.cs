using System.ComponentModel.DataAnnotations;

public class IdCardVerifyRequest
{
    [Required(ErrorMessage = "姓名不能为空")]
    public string RealName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "身份证号不能为空")]
    [MaxLength(18, ErrorMessage = "身份证号最长18位")]
    public string IdCard { get; set; } = string.Empty;
}

public class HostApplyRequest
{
    [Required(ErrorMessage = "真实姓名不能为空")]
    public string RealName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "身份证号不能为空")]
    public string IdCard { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "手机号不能为空")]
    public string Phone { get; set; } = string.Empty;
    
    public string? Email { get; set; }
    
    [Required(ErrorMessage = "省份不能为空")]
    public string Province { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "城市不能为空")]
    public string City { get; set; } = string.Empty;
    
    public string? District { get; set; }
    
    [Required(ErrorMessage = "详细地址不能为空")]
    public string Address { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "房源类型不能为空")]
    public string PropertyType { get; set; } = string.Empty;
    
    public int? RoomCount { get; set; }
    public int? BedCount { get; set; }
    public int? GuestCount { get; set; }
    
    [Required(ErrorMessage = "房源标题不能为空")]
    public string PropertyTitle { get; set; } = string.Empty;
    
    public string? PropertyDesc { get; set; }
    
    public List<string>? Amenities { get; set; }
    
    public List<string>? Images { get; set; }
}
