using Microsoft.Data.Sqlite;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace LibraryManagement.Data;

public class DatabaseInitializer
{
    private static string MigrationFolder = "Data\\Migrations";

    public static void Initialize()
    {
        ResetOutdatedDatabaseIfNeeded();

        using var connection = DatabaseConnection.CreateConnection();

        CreateMigrationHistoryTable(connection);

        string migrationPath = Path.Combine(AppContext.BaseDirectory, MigrationFolder);

        if (!Directory.Exists(migrationPath))
        {
            throw new DirectoryNotFoundException($"không tìm thấy thư mục migration: {migrationPath}");
        }

        var migrationFiles = Directory
            .GetFiles(migrationPath, "*.sql")
            .Select(file => new
            {
                Path = file,
                FileName = Path.GetFileName(file),
                Version = GetMigrationVersion(file)
            })
            .OrderBy(file => file.Version)
            .ToList();

        if (migrationFiles
            .GroupBy(file => file.Version)
            .Any(group => group.Count() > 1))
        {
            throw new InvalidOperationException("Phát hiện migration có version bị trùng");
        }

        foreach (var migration in migrationFiles)
        {
            if (!IsMigrationApplied(connection, migration.Version))
            {
                ApplyMigration(connection, migration.Path, migration.FileName, migration.Version);
            }
        }

        EnsureAdminAccount(connection);
    }

    private static void ResetOutdatedDatabaseIfNeeded()
    {
        string dbPath = DatabaseConnection.GetDatabasePath();
        if (!File.Exists(dbPath))
        {
            return;
        }

        bool isOutdated = false;
        using (var checkConnection = DatabaseConnection.CreateConnection())
        {
            using var command = checkConnection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'Users';";
            long userTableCount = (long)command.ExecuteScalar()!;
            if (userTableCount == 0)
            {
                isOutdated = true;
            }
        }

        if (isOutdated)
        {
            SqliteConnection.ClearAllPools();
            File.Delete(dbPath);
            if (File.Exists(dbPath + "-wal")) File.Delete(dbPath + "-wal");
            if (File.Exists(dbPath + "-shm")) File.Delete(dbPath + "-shm");
        }
    }

    private static void EnsureAdminAccount(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();

        command.CommandText = @"
            INSERT OR IGNORE INTO ""Users"" 
                (""UserCode"", ""Username"", ""PasswordHash"", ""FullName"", ""UserType"", ""MaxActiveCopies"", ""Status"")
            VALUES 
                ('NV000', 'admin', '123456', 'Quản trị viên', 'Admin', NULL, 'Active');
            UPDATE ""Users"" SET ""PasswordHash"" = '123456', ""Status"" = 'Active' WHERE ""Username"" = 'admin';";

        command.ExecuteNonQuery();
    }

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

    private static bool IsMigrationApplied(SqliteConnection connection, int version)
    {
        using var command = connection.CreateCommand();

        command.CommandText = @"SELECT COUNT(*) FROM SchemaMigrations WHERE Version = $version";

        command.Parameters.AddWithValue("$version", version);

        long count = (long)command.ExecuteScalar()!;

        return count > 0;
    }

    private static void ApplyMigration(SqliteConnection connection, string migrationFilePath, string migrationName,
        int version)
    {
        string sql = File.ReadAllText(migrationFilePath);
        using var transaction = connection.BeginTransaction();

        try
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = sql;

                command.ExecuteNonQuery();
            }

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
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }
}