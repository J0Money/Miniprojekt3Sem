using System;
using Microsoft.EntityFrameworkCore;
using MiniReddit.Api.Data;
using shared.Model;

namespace MiniReddit.Api.Service;

public class DataService
{
    private RedditContext db { get; }

    public DataService(RedditContext db)
    {
        this.db = db;
    }
    
    
    
    // GET /api/posts
    public List<Post> GetPosts() => 
    db.Posts
        .OrderByDescending(p => p.Timestamp)
        .Take(50)
        .ToList();
    
    // GET /api/posts/{id}
    public Post? GetPost(int id) =>
        db.Posts
            .Include(p => p.Comments) 
            .FirstOrDefault(p => p.PostId == id);
    
    // PUT /api/posts/{id}/upvote
    public bool UpvotePost(int postId)
    {
        var p = db.Posts.FirstOrDefault(p => p.PostId == postId);
        if (p == null) return false;
        p.Upvotes += 1;
        db.SaveChanges();
        return true;
    }

    // PUT /api/posts/{id}/downvote
    public bool DownvotePost(int id)
    {
        var post = db.Posts.FirstOrDefault(p => p.PostId == id);
        if (post is null) return false;

        post.Downvotes += 1;

        db.SaveChanges();
        return true;
    }
    
    // PUT /api/posts/{postId}/comments/{commentId}/upvote
    public bool UpvoteComment(int postId, int commentId)
    {
        var c = db.Comments.FirstOrDefault(x => x.CommentId == commentId && x.PostId == postId);
        if (c == null) return false;
        c.Upvotes += 1;
        db.SaveChanges();
        return true;
    }

    // PUT /api/posts/{postId}/comments/{commentId}/downvote
    public bool DownvoteComment(int postId, int commentId)
    {
        var c = db.Comments.FirstOrDefault(x => x.CommentId == commentId && x.PostId == postId);
        if (c == null) return false;
        c.Downvotes += 1;
        db.SaveChanges();
        return true;
    }
    
    // POST /api/posts
    public int CreatePost(string title, string author, string? content, string? url)
    {
        var post = new Post { Title = title, Author = author, Content = content, Url = url };
        db.Posts.Add(post);
        db.SaveChanges();
        return post.PostId;
    }
    
    // POST /api/posts/{id}/comments
    public int CreateComment(int postId, string author, string content)
    {
        var postExists = db.Posts.Any(p => p.PostId == postId);
        if (!postExists) return 0;
        var c = new Comment { Author = author, Content = content, PostId = postId, Timestamp = DateTime.UtcNow};
        db.Comments.Add(c);
        db.SaveChanges();
        return c.CommentId;
    }
    
    
}

