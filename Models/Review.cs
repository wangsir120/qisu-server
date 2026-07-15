namespace qisu_server.Models;

public class Review
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long UserId { get; set; }
    public long PropertyId { get; set; }
    public long HostId { get; set; }
    public byte Rating { get; set; }
    public decimal? CleanlinessRating { get; set; }
    public decimal? CommunicationRating { get; set; }
    public decimal? CheckinRating { get; set; }
    public decimal? AccuracyRating { get; set; }
    public decimal? LocationRating { get; set; }
    public decimal? ValueRating { get; set; }
    public string? Content { get; set; }
    public bool IsAnonymous { get; set; }
    public string? HostReply { get; set; }
    public DateTime? HostReplyTime { get; set; }
    public bool Status { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User? User { get; set; }
    public Order? Order { get; set; }
    public Property? Property { get; set; }
    public Host? Host { get; set; }
    public List<ReviewImage> Images { get; set; } = new();
    public List<ReviewReply> Replies { get; set; } = new();
}

public class ReviewImage
{
    public long Id { get; set; }
    public long ReviewId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }

    public Review? Review { get; set; }
}

public class ReviewReply
{
    public long Id { get; set; }
    public long ReviewId { get; set; }
    public long? HostId { get; set; }
    public long? UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Review? Review { get; set; }
    public Host? Host { get; set; }
    public User? User { get; set; }
}
