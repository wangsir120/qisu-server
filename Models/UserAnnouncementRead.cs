using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace qisu_server.Models;

[Table("user_announcement_reads")]
public class UserAnnouncementRead
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("announcement_id")]
    public long AnnouncementId { get; set; }

    [Column("read_at")]
    public DateTime ReadAt { get; set; } = DateTime.Now;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [ForeignKey("UserId")]
    public User? User { get; set; }

    [ForeignKey("AnnouncementId")]
    public Announcement? Announcement { get; set; }
}
