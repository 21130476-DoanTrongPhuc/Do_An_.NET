using LibraryManagement.Data;

using LibraryManagement.Repositories.Interfaces;
using System.Linq;   

using Microsoft.Data.Sqlite;
using LibraryManagement.DTO;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interfaces;

namespace LibraryManagement.Repositories;

public class BookRepository : IBookRepository
{
    
    private LibraryDbContext _context;
    
    public BookRepository(LibraryDbContext context)
    {
        _context = context;
    }
    
    public List<BookDto> GetAll()
    {
        return _context.Books
            .OrderByDescending(b => b.Id)
            .Select(b => new BookDto
            {
                BookCode = b.BookCode,
                ISBN = b.ISBN,
                Title = b.Title,
                Author = b.Author,
                CategoryName = b.Category != null ? b.Category.CategoryName : null,
                SoLuongB = b.Copies.Count(bc => bc.CirculationStatus == "Available")
            })
            .ToList();
    }

    public Book? GetById(int bookId)
    {
        throw new NotImplementedException();
    }

    public int Insert(Book book)
    {
        throw new NotImplementedException();
    }

    public bool Update(Book book)
    {
        throw new NotImplementedException();
    }

    public bool Delete(int bookId)
    {
        throw new NotImplementedException();
    }

    // Các phương thức GetById, Insert,
    // Update và Delete sẽ được triển khai tương tự.
}
