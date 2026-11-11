using LibraryManagement.Models;
using LibraryManagement.Repositories.Interfaces;
using LibraryManagement.Services.Interfaces;

namespace LibraryManagement.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public void AddBook(
        string isbn,
        string title,
        string author,
        int quantity
    )
    {
        // 1. Kiểm tra dữ liệu đầu vào
        if (string.IsNullOrWhiteSpace(isbn))
            throw new Exception("ISBN không được để trống!");

        if (string.IsNullOrWhiteSpace(title))
            throw new Exception("Tên sách không được để trống!");

        if (string.IsNullOrWhiteSpace(author))
            throw new Exception("Tên tác giả không được để trống!");

        if (quantity <= 0)
            throw new Exception("Số lượng sách phải lớn hơn 0!");

        // 2. Kiểm tra ISBN đã tồn tại
        // if (_bookRepository.ExistsByISBN(isbn.Trim()))
        //     throw new Exception("ISBN đã tồn tại!");

        // 3. Tạo đối tượng Book
        var book = new Book
        {
            ISBN = isbn.Trim(),
            Title = title.Trim(),
            Author = author.Trim(),
        };

        // 4. Lưu sách vào database
        // _bookRepository.Add(book);
    }
}