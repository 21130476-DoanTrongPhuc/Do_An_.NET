using LibraryManagement.Models;

namespace LibraryManagement.Repositories.Interfaces;

public interface IBookRepository
{
    bool ExistsByISBN(string isbn);

    void Add(Book book);
}