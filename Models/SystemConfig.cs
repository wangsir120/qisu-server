using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace qisu_server.Models;

[Table("system_configs")]
public class SystemConfig
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("config_key")]
    [MaxLength(100)]
    [Required]
    public string ConfigKey { get; set; } = string.Empty;

    [Column("config_value")]
    public string? ConfigValue { get; set; }

    [Column("description")]
    [MaxLength(500)]
    public string? Description { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
