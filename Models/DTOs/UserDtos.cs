using System.ComponentModel.DataAnnotations;

namespace qisu_server.Models.DTOs;

public class UserQueryRequest
{
    public string? Status { get; set; }
    public string? Keyword { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class UserListDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Avatar { get; set; }
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; }
}

public class CreateUserRequest
{
    [Required(ErrorMessage = "用户名不能为空")]
    [MaxLength(50, ErrorMessage = "用户名最长50个字符")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "密码不能为空")]
    [MinLength(6, ErrorMessage = "密码至少6个字符")]
    public string Password { get; set; } = string.Empty;

    public string? Nickname { get; set; }
    
    [Required(ErrorMessage = "手机号不能为空")]
    public string Phone { get; set; } = string.Empty;
    
    public string? Email { get; set; }
}

public class UpdateUserRequest
{
    public string? Nickname { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Status { get; set; }
}

public class ResetUserPasswordRequest
{
    [Required(ErrorMessage = "新密码不能为空")]
    [MinLength(6, ErrorMessage = "密码至少6个字符")]
    public string NewPassword { get; set; } = string.Empty;
}
