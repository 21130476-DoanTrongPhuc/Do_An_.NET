using Microsoft.Data.Sqlite;
using System;
using System.Globalization;
using System.IO;
using System.Linq;


namespace LibraryManagement.Data;

public class DatabaseInitializer
{
    private static string MigrationFolder = "Data\\Migrations";
    
    /* khởi tạo database và ap dụng các migration chưa chạy */
    public static void Initialize()
    {
        using var connection = DatabaseConnection.CreateConnection();
        
        /*Tạo bảng theo dõi migration*/
        CreateMigrationHistoryTable(connection);
        
        /* Lấy danh sách migration */
        string migrationPath = Path.Combine(AppContext.BaseDirectory, MigrationFolder);

        if (!Directory.Exists(migrationPath))
        {
            throw new DirectoryNotFoundException($"không tìm thấy thư mục migration: {migrationPath}");
        }
        
        var migrationFiles  = Directory
            .GetFiles(migrationPath, "*.sql")
            .Select(file => new
            {
                Path = file,
                FileName = Path.GetFileName(file),
                Version = GetMigrationVersion(file)
            })
            .OrderBy(file => file.Version)
            .ToList();
        
        /* Kiểm tra version trùng lạp */
        if (migrationFiles
            .GroupBy(file => file.Version)
            .Any(group => group.Count() > 1))
        {
            throw new InvalidOperationException("Phát hiện migration có version bị trùng");
        }
        
        /* chạy những migration chưa được áp dụng */
        foreach (var migration in migrationFiles)
        {
            if (!IsMigrationApplied(connection, migration.Version))
            {
                ApplyMigration(connection, migration.Path, migration.FileName, migration.Version);
            }
        }
    }
    
    /* Tao bảng lưu lịch sử migration */
    private static void CreateMigrationHistoryTable(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();

        command.CommandText = @"CREATE TABLE IF NOT EXISTS SchemaMigrations 
                                (
                                    Version INTEGER PRIMARY KEY,
                                    MigrationName TEXT NOT NULL,
                                    AppliedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                                );";
        
        command.ExecuteNonQuery();
    }
    
    /* Lấy version từ tên file migration
       Ví dụ : 001_CreateTable.sql -> 1
     */
    private static int GetMigrationVersion(string filePath)
    {
        string fileName = Path.GetFileNameWithoutExtension(filePath);

        string versionPath = fileName.Split('_')[0];

        if (!int.TryParse(versionPath, NumberStyles.None, CultureInfo.InvariantCulture, out int version) ||
            version <= 0)
        {
            throw new InvalidOperationException($"Tên migration không hợp lệ: {fileName}");
        }
        return version;
    }
    
    /* Kiểm tra migration đã được áp dụng chưa*/
    private static bool IsMigrationApplied(SqliteConnection connection, int version)
    {
        using var command = connection.CreateCommand();

        command.CommandText = @"SELECT COUNT(*) FROM SchemaMigrations WHERE  Version = $version";

        command.Parameters.AddWithValue("$version", version);

        long count = (long) command.ExecuteScalar();

        return count > 0;
    }
    
    /* Đọc và thực thi migration trong transaction */
    private static void ApplyMigration(SqliteConnection connection, string migrationFilePath, string migrationName,
        int version)
    {
        string sql = File.ReadAllText(migrationFilePath);
        using var transaction = connection.BeginTransaction();

        try
        {
            /* Thực thi SQL migration */
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = sql;

                command.ExecuteNonQuery();
            }
            
            /* Ghi nhận migration đã chạy */
            using (var historyCommand = connection.CreateCommand())
            {
                historyCommand.Transaction = transaction;

                historyCommand.CommandText = @"INSERT INTO SchemaMigrations (Version, MigrationName)
                                               VALUES ($version, $name)";
                
                historyCommand.Parameters.AddWithValue("$version", version);
                
                historyCommand.Parameters.AddWithValue("$name", migrationName);
                
                historyCommand.ExecuteNonQuery();
            }
            
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();
            throw;
        }
    }
}