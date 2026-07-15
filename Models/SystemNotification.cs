using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace qisu_server.Models;

[Table("system_notifications")]
public class SystemNotification
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("content")]
    public string? Content { get; set; }

    [MaxLength(20)]
    [Column("type")]
    public string Type { get; set; } = "info";

    [Column("target_user_id")]
    public long? TargetUserId { get; set; }

    [MaxLength(20)]
    [Column("target_role")]
    public string? TargetRole { get; set; }

    [Column("is_read")]
    public bool IsRead { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
