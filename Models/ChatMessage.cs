using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace qisu_server.Models;

[Table("cs_messages")]
public class ChatMessage
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("conversation_id")]
    [MaxLength(50)]
    public string? ConversationId { get; set; }

    [Column("sender_id")]
    public long SenderId { get; set; }

    [Column("receiver_id")]
    public long? ReceiverId { get; set; }

    [Column("content")]
    [Required]
    public string Content { get; set; } = string.Empty;

    [Column("message_type")]
    [MaxLength(20)]
    public string MessageType { get; set; } = "text";

    [Column("is_read")]
    public bool IsRead { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [ForeignKey("SenderId")]
    public User? Sender { get; set; }

    [ForeignKey("ReceiverId")]
    public User? Receiver { get; set; }
}

[Table("cs_conversations")]
public class ChatConversation
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("conversation_id")]
    [MaxLength(50)]
    public string ConversationId { get; set; } = Guid.NewGuid().ToString("N");

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("admin_id")]
    public long? AdminId { get; set; }

    [Column("last_message")]
    public string? LastMessage { get; set; }

    [Column("last_message_time")]
    public DateTime? LastMessageTime { get; set; }

    [Column("unread_count")]
    public int UnreadCount { get; set; } = 0;

    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = "active";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [ForeignKey("UserId")]
    public User? User { get; set; }

    [ForeignKey("AdminId")]
    public User? Admin { get; set; }
}
