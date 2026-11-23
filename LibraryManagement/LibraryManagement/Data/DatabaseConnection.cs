using Microsoft.Data.Sqlite;

namespace LibraryManagement.Data;

public class DatabaseConnection
{
    private const string DatabaseFileName = "library.db";
    private const string DatabaseFolder = "Database";

    /* DB luôn nằm trong thư mục chạy của app (bin/... hoặc thư mục publish) */
    public static string GetDatabasePath()
    {
        string databaseDirectory = Path.Combine(AppContext.BaseDirectory, DatabaseFolder);

        Directory.CreateDirectory(databaseDirectory);

        return Path.Combine(databaseDirectory, DatabaseFileName);
    }

    public static string GetConnectionString()
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = GetDatabasePath(),
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared,
            Pooling = true,
            ForeignKeys = true,
            DefaultTimeout = 30
        };

        return builder.ToString();
    }

    /* Tạo và mở connection mới. Caller phải dispose. */
    public static SqliteConnection CreateConnection()
    {
        var connection = new SqliteConnection(GetConnectionString());

        try
        {
            connection.Open();
            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }

    public static bool TestConnection()
    {
        using var connection = CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1";
        object? result = command.ExecuteScalar();
        return Convert.ToInt32(result) == 1;
    }
}