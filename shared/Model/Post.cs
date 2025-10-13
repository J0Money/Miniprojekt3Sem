namespace shared.Model;
public class Post
{
    public int PostId { get; set; }

    public string Title { get; set; } = "";
    
    public string Author { get; set; } = "";
    
    public string? Content { get; set; }
    
    public string? Url { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public List<Comment> Comments { get; set; } = new();
    
    public int Upvotes { get; set; }
    
    public int Downvotes { get; set; }
}