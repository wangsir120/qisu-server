using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace qisu_server.Models;

[Table("announcements")]
public class Announcement
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("title")]
    [MaxLength(200)]
    [Required]
    public string Title { get; set; } = string.Empty;

    [Column("content")]
    [Required]
    public string Content { get; set; } = string.Empty;

    [Column("type")]
    [MaxLength(20)]
    public string Type { get; set; } = "notice";

    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = "draft";

    [Column("is_top")]
    public bool IsTop { get; set; } = false;

    [Column("start_time")]
    public DateTime? StartTime { get; set; }

    [Column("end_time")]
    public DateTime? EndTime { get; set; }

    [Column("view_count")]
    public int ViewCount { get; set; } = 0;

    [Column("created_by")]
    public long? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
