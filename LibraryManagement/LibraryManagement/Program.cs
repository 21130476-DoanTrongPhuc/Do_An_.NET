using LibraryManagement.Data;
using LibraryManagement.Forms;
using Microsoft.Data.Sqlite;

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
            ApplicationConfiguration.Initialize();
            Application.Run(new LibraryManagement.Forms.HomePage());
        }
    }
}
