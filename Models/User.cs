namespace qisu_server.Models;

public class User
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Avatar { get; set; }
    public string? Nickname { get; set; }
    public string? Gender { get; set; }
    public string? IdCard { get; set; }
    public bool IsVerified { get; set; }
    public byte Status { get; set; } = 1;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
