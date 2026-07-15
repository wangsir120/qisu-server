using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace qisu_server.Models;

[Table("banners")]
public class Banner
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("title")]
    [MaxLength(200)]
    public string? Title { get; set; }

    [Column("subtitle")]
    [MaxLength(500)]
    public string? Subtitle { get; set; }

    [Column("image_url")]
    [MaxLength(500)]
    [Required]
    public string ImageUrl { get; set; } = string.Empty;

    [Column("link_url")]
    [MaxLength(500)]
    public string? LinkUrl { get; set; }

    [Column("link_type")]
    [MaxLength(20)]
    public string? LinkType { get; set; }

    [Column("gradient")]
    [MaxLength(200)]
    public string? Gradient { get; set; }

    [Column("position")]
    [MaxLength(20)]
    public string Position { get; set; } = "home";

    [Column("sort_order")]
    public int SortOrder { get; set; } = 0;

    [Column("status")]
    public bool Status { get; set; } = true;

    [Column("start_time")]
    public DateTime? StartTime { get; set; }

    [Column("end_time")]
    public DateTime? EndTime { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
