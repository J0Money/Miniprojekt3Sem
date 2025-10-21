using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

using shared.Model;

namespace kreddit_app.Data;

public class ApiService
{
    private readonly HttpClient http;
    private readonly IConfiguration configuration;
    private readonly string baseAPI = "";

    public ApiService(HttpClient http, IConfiguration configuration)
    {
        this.http = http;
        this.configuration = configuration;
        this.baseAPI = configuration["base_api"];
    }

    public async Task<Post[]> GetPosts()
    {
        string url = $"{baseAPI}posts/";
        return await http.GetFromJsonAsync<Post[]>(url);
    }

    public async Task<Post> GetPost(int id)
    {
        string url = $"{baseAPI}posts/{id}/";
        return await http.GetFromJsonAsync<Post>(url);
    }
    
    public async Task UpvotePost(int id)
    {
        var res = await http.PutAsync($"{baseAPI}posts/{id}/upvote", null);
        res.EnsureSuccessStatusCode();
    }

    public async Task DownvotePost(int id)
    {
        var res = await http.PutAsync($"{baseAPI}posts/{id}/downvote", null);
        res.EnsureSuccessStatusCode();
    }
    
    public async Task UpvoteComment(int postId, int commentId)
    {
        await http.PutAsync($"{baseAPI}posts/{postId}/comments/{commentId}/upvote", null);
       
    }

    public async Task DownvoteComment(int postId, int commentId)
    {
        await http.PutAsync($"{baseAPI}posts/{postId}/comments/{commentId}/downvote", null);
       
    }
    
    public async Task<int> CreatePost(string title, string author, string? content, string? url)
    {
        var resp = await http.PostAsJsonAsync($"{baseAPI}posts", new { title, author, content, url });
        resp.EnsureSuccessStatusCode();
        var dto = await resp.Content.ReadFromJsonAsync<IdResponse>();
        return dto!.Id;
    }

    public async Task<CommentDto?> CreateComment(int postId, string author, string content)
    {
        var resp = await http.PostAsJsonAsync($"{baseAPI}posts/{postId}/comments", new { author, content });
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<CommentDto>();
    }

    public record IdResponse(int Id);

    public record CommentDto(int Id, int PostId, string Content, string Author, DateTime Timestamp, int Upvotes, int Downvotes);

}
