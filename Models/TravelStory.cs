namespace qisu_server.Models;

public class TravelStory
{
    public long Id { get; set; }
    public long? UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }
    public string StoryType { get; set; } = "travel_story";
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public bool Status { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User? User { get; set; }
}
