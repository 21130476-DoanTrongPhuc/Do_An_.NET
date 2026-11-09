using LibraryManagement.Data;
using LibraryManagement.Forms.LibrarianFormMain;
using LibraryManagement.Repositories;
using LibraryManagement.Services;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // 1. Tạo đường dẫn database
        string appData = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData
        );

        string dbDirectory = Path.Combine(
            appData,
            "LibraryManagement"
        );

        Directory.CreateDirectory(dbDirectory);

        string dbPath = Path.Combine(
            dbDirectory,
            "library.db"
        );

        // 2. Cấu hình SQLite
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        // 3. Khởi tạo database
        using (var db = new LibraryDbContext(options))
        {
            db.Database.EnsureCreated();
        }

        // 4. Khởi tạo các đối tượng
        var bookRepository = new BookRepository(options);

        var bookService = new BookService(bookRepository);

        // 5. Mở Form
        // Application.Run(new BookCreateForm(bookService));
        Application.Run(new LibrarianMainForm());
    }
}