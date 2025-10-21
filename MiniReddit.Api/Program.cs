using Microsoft.EntityFrameworkCore;
using MiniReddit.Api.Data;
using shared.Model;
using MiniReddit.Api.Service;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(o => o.AddPolicy("Wasm", p =>
    p.WithOrigins("https://localhost:7228", "http://localhost:5202")
        .AllowAnyHeader()
        .AllowAnyMethod()
));

// DbContext SQLite
builder.Services.AddDbContext<RedditContext>(opt => opt.UseSqlite(builder.Configuration.GetConnectionString("ContextSQLite")));

// DI DataService
builder.Services.AddScoped<DataService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MiniReddit.Api.Data.RedditContext>();
    
    db.Database.Migrate();

    if (!db.Posts.Any())
    {
        var now = DateTime.UtcNow;

        var p1 = new shared.Model.Post {
            Title = "Velkommen til MiniReddit",
            Author = "Joe",
            Content = "Første post – prøv at stemme og kommentere!",
            Url = null,
            Timestamp = now.AddMinutes(-30),
            Upvotes = 3,
            Downvotes = 0
        };

        var p2 = new shared.Model.Post {
            Title = "Link-post eksempel",
            Author = "Mads",
            Content = null,
            Url = "https://example.org",
            Timestamp = now.AddMinutes(-20),
            Upvotes = 1,
            Downvotes = 0
        };

        db.Posts.AddRange(p1, p2);
        db.SaveChanges();
        
        db.Comments.AddRange(
            new shared.Model.Comment {
                PostId = p1.PostId, Author = "alice",
                Content = "Ser godt ud!", Timestamp = now.AddMinutes(-25),
                Upvotes = 2, Downvotes = 0
            },
            new shared.Model.Comment {
                PostId = p1.PostId, Author = "bob",
                Content = "Husk Minimal API 👍", Timestamp = now.AddMinutes(-22),
                Upvotes = 1, Downvotes = 0
            }
        );

        db.SaveChanges();
    }
}

app.UseHttpsRedirection();
app.UseCors("Wasm");


static PostDto ToDto(Post p) =>
    new(p.PostId, p.Title, p.Content, p.Url, p.Author, p.Timestamp, p.Upvotes, p.Downvotes);

app.MapGet("/api/posts", (DataService s) =>
    s.GetPosts().Select(ToDto)
);

app.MapGet("/api/posts/{id:int}", (DataService s, int id) =>
{
    var p = s.GetPost(id);
    return p is null ? Results.NotFound() : Results.Ok(p);
});

app.MapPut("/api/posts/{id:int}/upvote", (DataService s, int id) =>
{
    if (!s.UpvotePost(id)) return Results.NotFound();
    return Results.Ok(s.GetPost(id)!); 
});

app.MapPut("/api/posts/{id:int}/downvote", (DataService s, int id) =>
{
    if (!s.DownvotePost(id)) return Results.NotFound();
    return Results.Ok(s.GetPost(id)!);
});


app.MapPut("/api/posts/{postId:int}/comments/{commentId:int}/upvote",
    (DataService service, int postId, int commentId) =>
        service.UpvoteComment(postId, commentId) ? Results.NoContent() : Results.NotFound());

app.MapPut("/api/posts/{postId:int}/comments/{commentId:int}/downvote",
    (DataService service, int postId, int commentId) =>
        service.DownvoteComment(postId, commentId) ? Results.NoContent() : Results.NotFound());

app.MapPost("/api/posts", (DataService s, NewPostData d) =>
{
    if (string.IsNullOrWhiteSpace(d.Title) || string.IsNullOrWhiteSpace(d.Author))
        return Results.BadRequest("Title og Author er påkrævet.");

    var hasText = !string.IsNullOrWhiteSpace(d.Content);
    var hasUrl  = !string.IsNullOrWhiteSpace(d.Url);
    if (hasText == hasUrl) 
        return Results.BadRequest("Angiv enten Content eller Url.");

    var id = s.CreatePost(d.Title.Trim(), d.Author.Trim(),
        hasText ? d.Content!.Trim() : null,
        hasUrl  ? d.Url!.Trim()     : null);

    return Results.Created($"/api/posts/{id}", new { id });
});;


app.MapPost("/api/posts/{id:int}/comments", (DataService s, int id, NewCommentData data) =>
{
    if (string.IsNullOrWhiteSpace(data.Author) || string.IsNullOrWhiteSpace(data.Content))
        return Results.BadRequest("Author and Content are required.");

    var commentId = s.CreateComment(id, data.Author.Trim(), data.Content.Trim());
    if (commentId == 0) return Results.NotFound($"Post {id} not found");

    var dto = new CommentDto(commentId, id, data.Content.Trim(), data.Author.Trim(), DateTime.UtcNow, 0, 0);
    return Results.Created($"/api/posts/{id}/comments/{commentId}", dto);
});

app.Run();

public record PostDto(
    int PostId, string Title, string? Content, string? Url,
    string Author, DateTime Timestamp, int Upvotes, int Downvotes
);
public record NewPostData(string Title, string Author, string? Content, string? Url);
public record NewCommentData(string Author, string Content);

public record CommentDto(int Id, int PostId, string Content, string Author, DateTime Timestamp, int Upvotes, int Downvotes);
