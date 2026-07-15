using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace qisu_server.Models;

[Table("messages")]
public class Message
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("title")]
    [MaxLength(200)]
    public string? Title { get; set; }

    [Column("content")]
    public string? Content { get; set; }

    [Column("type")]
    [MaxLength(20)]
    public string? Type { get; set; } = "system";

    [Column("is_read")]
    public bool IsRead { get; set; } = false;

    [Column("related_id")]
    public long? RelatedId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
