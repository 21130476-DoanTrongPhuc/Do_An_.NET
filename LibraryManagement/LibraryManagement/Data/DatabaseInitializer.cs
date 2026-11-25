using Microsoft.Data.Sqlite;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace LibraryManagement.Data;

public class DatabaseInitializer
{
    private static readonly string MigrationFolder = Path.Combine("Data", "Migrations");

    private static readonly object InitLock = new();
    private static bool _initialized;

    public static void Initialize()
    {
        lock (InitLock)
        {
            if (_initialized) return;
            InitializeCore();
            _initialized = true;
        }
    }

    private static void InitializeCore()
    {
        using var connection = DatabaseConnection.CreateConnection();

        /* WAL giúp đọc/ghi đồng thời tốt hơn với Cache=Shared.
           journal_mode là persistent (chỉ cần set 1 lần), busy_timeout là per-connection. */
        using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = @"
                PRAGMA journal_mode = WAL;
                PRAGMA synchronous = NORMAL;
                PRAGMA busy_timeout = 30000;";
            pragma.ExecuteNonQuery();
        }

        CreateMigrationHistoryTable(connection);

        string migrationPath = Path.Combine(AppContext.BaseDirectory, MigrationFolder);

        if (!Directory.Exists(migrationPath))
        {
            throw new DirectoryNotFoundException($"Không tìm thấy thư mục migration: {migrationPath}");
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

        // EnsureAdminAccount(connection);
    }

    private static void CreateMigrationHistoryTable(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();

        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS SchemaMigrations
            (
                Version       INTEGER PRIMARY KEY,
                MigrationName TEXT NOT NULL UNIQUE,
                AppliedAt     TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            );";

        command.ExecuteNonQuery();
    }

    private static int GetMigrationVersion(string filePath)
    {
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        string versionPart = fileName.Split('_')[0];

        if (!int.TryParse(versionPart, NumberStyles.None, CultureInfo.InvariantCulture, out int version) ||
            version <= 0)
        {
            throw new InvalidOperationException($"Tên migration không hợp lệ: {fileName}");
        }

        return version;
    }

    private static bool IsMigrationApplied(SqliteConnection connection, int version)
    {
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT COUNT(*) FROM SchemaMigrations WHERE Version = $version";
        command.Parameters.AddWithValue("$version", version);

        long count = (long)command.ExecuteScalar()!;
        return count > 0;
    }

    private static void ApplyMigration(
        SqliteConnection connection,
        string migrationFilePath,
        string migrationName,
        int version)
    {
        string sql = File.ReadAllText(migrationFilePath);

        if (string.IsNullOrWhiteSpace(sql))
            return;

        bool disableForeignKeys = sql.TrimStart().StartsWith(
            "PRAGMA foreign_keys = OFF;",
            StringComparison.OrdinalIgnoreCase);

        if (disableForeignKeys)
            SetForeignKeysEnabled(connection, false);

        try
        {
            using var transaction = connection.BeginTransaction();

            try
            {
                // Microsoft.Data.Sqlite hỗ trợ thực thi nhiều câu SQL
                // trong cùng một CommandText.
                // Không cần tự tách bằng SplitSqlStatements().
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = sql;

                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (SqliteException ex)
                    {
                        throw new InvalidOperationException(
                            $"Lỗi khi thực thi migration!\n\n" +
                            $"Migration: {migrationName}\n" +
                            $"Version: {version}\n" +
                            $"File: {migrationFilePath}\n" +
                            $"SQLite Error: {ex.Message}\n\n" +
                            $"Nội dung SQL:\n{sql}",
                            ex);
                    }
                }

                if (disableForeignKeys)
                    EnsureForeignKeysAreValid(connection, transaction);

                // Lưu lịch sử migration
                using (var historyCommand = connection.CreateCommand())
                {
                    historyCommand.Transaction = transaction;

                    historyCommand.CommandText = @"
                        INSERT INTO SchemaMigrations
                            (Version, MigrationName)
                        VALUES
                            ($version, $name);";

                    historyCommand.Parameters.AddWithValue("$version", version);
                    historyCommand.Parameters.AddWithValue("$name", migrationName);

                    historyCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        finally
        {
            if (disableForeignKeys)
                SetForeignKeysEnabled(connection, true);
        }
    }

    private static void SetForeignKeysEnabled(SqliteConnection connection, bool enabled)
    {
        using var command = connection.CreateCommand();
        command.CommandText = enabled
            ? "PRAGMA foreign_keys = ON;"
            : "PRAGMA foreign_keys = OFF;";
        command.ExecuteNonQuery();
    }

    private static void EnsureForeignKeysAreValid(
        SqliteConnection connection,
        SqliteTransaction transaction)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "PRAGMA foreign_key_check;";

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            throw new InvalidOperationException(
                $"Migration tạo vi phạm khóa ngoại tại bảng '{reader.GetString(0)}'.");
        }
    }

    /*private static void EnsureAdminAccount(SqliteConnection connection)
    {
        // Câu lệnh 1: Chèn nếu chưa có
        using (var insertCommand = connection.CreateCommand())
        {
            insertCommand.CommandText = @"
            INSERT OR IGNORE INTO ""Users""
                (""UserCode"", ""Username"", ""PasswordHash"", ""FullName"", ""UserType"", ""MaxActiveCopies"", ""Status"")
            VALUES
                ('NV000', 'admin', '123456', 'Quản trị viên', 'Librarian', NULL, 'Active');";
            insertCommand.ExecuteNonQuery();
        }

        // Câu lệnh 2: Cập nhật thông tin (nếu cần)
        using (var updateCommand = connection.CreateCommand())
        {
            updateCommand.CommandText = @"
            UPDATE ""Users"" 
            SET ""PasswordHash"" = '123456', ""Status"" = 'Active' 
            WHERE ""Username"" = 'admin';";
            updateCommand.ExecuteNonQuery();
        }
    }*/
}