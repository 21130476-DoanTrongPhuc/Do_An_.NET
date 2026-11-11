using LibraryManagement.Data;
using LibraryManagement.Forms;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
#if DEBUG
            // CHỈ DÙNG KHI DEV: xóa DB cũ để migration luôn chạy lại từ đầu
            string dbPath = DatabaseConnection.GetDatabasePath();
            SqliteConnection.ClearAllPools();
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
#endif
            DatabaseInitializer.Initialize();
            
            var services = new ServiceCollection();

            services.AddDbContextFactory<LibraryDbContext>(
                options =>
                    options.UseSqlite(
                        DatabaseConnection.CreateConnection()
                    )
            );

            // Các đăng ký Repository và Service sẽ thêm ở đây.

            var serviceProvider = services.BuildServiceProvider();
            
            Application.Run(new LoginForm());
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Không thể khởi tạo ứng dụng!\n\n" +
                $"DB: {DatabaseConnection.GetDatabasePath()}\n\n" +
                $"Message: {ex.Message}\n\n" +
                $"StackTrace:\n{ex.StackTrace}",
                "Application Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}