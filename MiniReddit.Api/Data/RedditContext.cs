using Microsoft.EntityFrameworkCore;
using shared.Model;

namespace MiniReddit.Api.Data;

public class RedditContext : DbContext
{
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    
    public RedditContext(DbContextOptions<RedditContext> options)
        : base(options)
    {
        
    }
    
    
}