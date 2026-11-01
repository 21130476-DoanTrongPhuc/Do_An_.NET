namespace LibraryManagement.Services.Interfaces;

public interface IBookService
{
    void AddBook(
        string isbn,
        string title,
        string author,
        int quantity
    );
}