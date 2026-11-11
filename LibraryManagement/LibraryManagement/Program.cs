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

        try
        {
            // Khởi tạo database
            DatabaseInitializer.Initialize();
            
            // Mở màn hình đăng nhập
            // Application.Run(new LoginForm());
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Không thể khởi tạo ứng dụng!\n\n" +
                $"Message: {ex.Message}\n\n" +
                $"Type: {ex.GetType().Name}\n\n" +
                $"StackTrace:\n{ex.StackTrace}",
                "Application Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            
        }
    }
}