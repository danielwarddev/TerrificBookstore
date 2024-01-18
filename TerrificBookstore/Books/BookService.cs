using Microsoft.EntityFrameworkCore;
using TerrificBookstore.Database;

namespace TerrificBookstore.API.Books;

public interface IBookService
{
    Task LikeBook(int bookId, int userId);
    Task ReviewBook(int bookId, int userId, string reviewContent);
}

public class BookService : IBookService
{
    private readonly BookstoreContext _context;

    public BookService(BookstoreContext context)
    {
        _context = context;
    }
    
    public async Task LikeBook(int bookId, int userId)
    {
        var existingBookLike = await _context.BookLikes.FirstOrDefaultAsync(x => x.GutendexBookId == bookId && x.UserId == userId);
        if (existingBookLike != null)
        {
            return;
        }

        await _context.BookLikes.AddAsync(new BookLike()
        {
            GutendexBookId = bookId,
            UserId = userId
        });
        _context.SaveChanges();
    }

    public async Task ReviewBook(int bookId, int userId, string reviewContent)
    {
        var existingBookReview = await _context.BookReviews.FirstOrDefaultAsync(x => x.GutendexBookId == bookId && x.UserId == userId);
        
        if (existingBookReview != null)
        {
            existingBookReview.ReviewContent = reviewContent;
        }
        else
        {
            await _context.BookReviews.AddAsync(new BookReview()
            {
                GutendexBookId = bookId,
                UserId = userId,
                ReviewContent = reviewContent
            });
        }

        await _context.SaveChangesAsync();
    }
}