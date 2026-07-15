using System.ComponentModel.DataAnnotations;

namespace qisu_server.Models.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "用户名不能为空")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "密码不能为空")]
    public string Password { get; set; } = string.Empty;

    public string? Captcha { get; set; }

    public bool Remember { get; set; }
}

public class RegisterRequest
{
    [Required(ErrorMessage = "用户名不能为空")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "密码不能为空")]
    [MinLength(6, ErrorMessage = "密码长度至少6位")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "手机号不能为空")]
    [Phone(ErrorMessage = "手机号格式不正确")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "验证码不能为空")]
    [StringLength(6, MinimumLength = 4, ErrorMessage = "验证码长度不正确")]
    public string VerifyCode { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    [Required(ErrorMessage = "手机号不能为空")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "新密码不能为空")]
    [MinLength(6, ErrorMessage = "密码长度至少6位")]
    public string NewPassword { get; set; } = string.Empty;
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserData? User { get; set; }
    public string? Token { get; set; }
}

public class UserData
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Avatar { get; set; }
    public string? Nickname { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Gender { get; set; }
    public string? IdCard { get; set; }
    public bool IsVerified { get; set; }
    public string? Role { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class CaptchaResponse
{
    public string CaptchaId { get; set; } = string.Empty;
    public string CaptchaText { get; set; } = string.Empty;
}

public class SendSmsCodeRequest
{
    [Required(ErrorMessage = "手机号不能为空")]
    [Phone(ErrorMessage = "手机号格式不正确")]
    public string Phone { get; set; } = string.Empty;
}

public class VerifySmsCodeRequest
{
    [Required(ErrorMessage = "手机号不能为空")]
    [Phone(ErrorMessage = "手机号格式不正确")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "验证码不能为空")]
    [StringLength(6, MinimumLength = 4, ErrorMessage = "验证码长度不正确")]
    public string VerifyCode { get; set; } = string.Empty;
}
