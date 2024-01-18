using Microsoft.EntityFrameworkCore;

namespace TerrificBookstore.Database;

public class BookstoreContext : DbContext
{
    public DbSet<BookLike> BookLikes { get; set; }
    public DbSet<BookReview> BookReviews { get; set; }

    public BookstoreContext() { }
    public BookstoreContext(DbContextOptions<BookstoreContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }
    
    public override int SaveChanges()
    {
        return base.SaveChanges();
    }
}