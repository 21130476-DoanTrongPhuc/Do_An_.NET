using LibraryManagement.DTO;
using LibraryManagement.Models;

namespace LibraryManagement.Repositories.Interfaces;

public interface IBookRepository
{
    List<BookDto> GetAll();

    Book? GetById(int bookId);

    int Insert(Book book);

    bool Update(Book book);

    bool Delete(int bookId);
}
