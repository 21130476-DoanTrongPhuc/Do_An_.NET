using Microsoft.Data.Sqlite;
using System.IO;

namespace LibraryManagement.Data;

public class DatabaseConnection
{
    private static string DatabaseFileName = "library.db";
    private static string DatabaseFolder = "Database";

    private static string FindSolutionDirectory()
    {
        DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null)
        {
            FileInfo[] solutionFiles = directory.GetFiles("*.sln");

            if (solutionFiles.Length > 0)
            {
                return directory.FullName;
            }
            
            directory = directory.Parent;

        }
        throw new DirectoryNotFoundException("Không tìm thấy thư mục chúa file LibraryManagement.sln");
    }

    /* Xác định đường dẫn lưu database */
    public static string GetDatabasePath()
    {
        string solutionDirectory = FindSolutionDirectory();
        
        string databaseDirectory = Path.Combine(solutionDirectory, DatabaseFolder);

        /* Tạo thư mục nếu chưa tồn tại */
        Directory.CreateDirectory(databaseDirectory);

        return Path.Combine(databaseDirectory, DatabaseFileName);
    }

    /* Xây dựng connection string cho SQLite */
    private static string GetConnectionString()
    {
        var connectionStringBuilder = new SqliteConnectionStringBuilder
        {
            DataSource = GetDatabasePath(),

            /* Tự động tạo database nếu chưa tồn tại */
            Mode = SqliteOpenMode.ReadWriteCreate,

            /* Cho phép nhiều kết nối sử dụng chung cache */
            Cache = SqliteCacheMode.Shared,

            /* Bật connection pooling */
            Pooling = true,

            /* Bật kiểm tra khóa ngoại */
            ForeignKeys = true,

            /* Thời gian chờ khi database bị khóa */
            DefaultTimeout = 30
        };

        return connectionStringBuilder.ToString();
    }
    
    /*
     * Tạo và mờ một kết nối mới
     * Người gọi phải dispose connection sai khi sử dụng
     */
    public static SqliteConnection CreateConnection()
    {
        var connection = new SqliteConnection(GetConnectionString());

        try
        {
            connection.Open();
            return connection;
        }
        catch (Exception e)
        {
            connection.Dispose();
            throw;
        }
    }

    public static bool TestConnection()
    {
        try
        {
            using var connection = CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            object? result = command.ExecuteScalar();
            return Convert.ToInt32(result) == 1;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

}