using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Repositories;

public class BookRepository : IBookRepository
{
    private readonly DbContextOptions<LibraryDbContext> _options;

    public BookRepository(
        DbContextOptions<LibraryDbContext> options
    )
    {
        _options = options;
    }

    public bool ExistsByISBN(string isbn)
    {
        using var db = new LibraryDbContext(_options);

        return db.Books.Any(b => b.ISBN == isbn);
    }

    public void Add(Book book)
    {
        using var db = new LibraryDbContext(_options);

        db.Books.Add(book);

        db.SaveChanges();
    }
}