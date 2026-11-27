using LibraryManagement.Controls;
using LibraryManagement.Data;
using LibraryManagement.Forms;
using LibraryManagement.Forms.BookFormReturn;
using LibraryManagement.Forms.HomePage;
using LibraryManagement.Forms.LibraryCardManagement;
using LibraryManagement.Forms.LoginForm;
using LibraryManagement.Forms.OverdueBooks;
using LibraryManagement.Forms.OverdueBooks;
using LibraryManagement.Forms.ReaderManagement;
using LibraryManagement.Forms.ReservationManagement;
using LibraryManagement.Helpers;
using LibraryManagement.Repositories;
using LibraryManagement.Repositories.Interafaces;
using LibraryManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            // Chạy các file migration (001, 002, 003...) chưa được áp dụng
            DatabaseInitializer.Initialize();

            var services = new ServiceCollection();

            // Mỗi lần dùng sẽ tạo một DbContext riêng, mỗi context tự mở connection riêng
            services.AddDbContextFactory<LibraryDbContext>(options =>
                options.UseSqlite(DatabaseConnection.GetConnectionString()));

            // Các đăng ký Repository và Service sẽ thêm ở đây (bước sau).
            services.AddTransient<ReturnedBookRepository, ReturnedBookRepositoryImpl>();
            services.AddTransient<ReturnedBookServiceImpl>();
            
            services.AddTransient<FineRepository, FineRepositoryImpl>();

            services.AddTransient<ManageOverdueBookRepository, ManageOverdueRepositoryImpl>();
            services.AddTransient<ManageOverdueServiceImpl>();

            services.AddTransient<FineRepository, FineRepositoryImpl>();
            services.AddTransient<FineService>();

            services.AddTransient<StatisticalReportRepository, StatisticalReportRepositoryImpl>();
            services.AddTransient<StatisticalReportService>();
            
            services.AddTransient<ReaderManagementRepository, ReaderManagementRepositoryImpl>();
            services.AddTransient<ReaderManagementService>();
            
            services.AddTransient<ReservationRepository, ReservationRepositoryImpl>();
            services.AddTransient<ReservationService>();
            
            services.AddTransient<LibraryCardRepository, LibraryCardRepositoryImpl>();
            services.AddTransient<LibraryCardService>();
            
            services.AddTransient<LoginForm>();
            services.AddTransient<BookFormReturn>();
            services.AddTransient<OverdueBooks>();
            services.AddTransient<StatisticalReportControl>();
            services.AddTransient<ReaderManagementControl>();
            services.AddTransient<ReservationManagementControl>();
            services.AddTransient<LibraryCardManagementControl>();

            
            AppServices.Provider = services.BuildServiceProvider();

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