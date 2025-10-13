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
//prøver at fikse kommentar
    public async Task<CommentDto?> CreateComment(int postId, string author, string content)
    {
        var url = $"{baseAPI}posts/{postId}/comments";
        var resp = await http.PostAsJsonAsync(url, new { author, content });

        var body = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException($"POST /comments failed {(int)resp.StatusCode}: {body}");

        return JsonSerializer.Deserialize<CommentDto>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
        var body = await resp.Content.ReadFromJsonAsync<Dictionary<string,int>>();
        return body!["id"];
    }
    
//prøver at fikse kommentar
    public record CommentDto(int Id, int PostId, string Content, string Author, DateTime Timestamp, int Upvotes, int Downvotes);

}
