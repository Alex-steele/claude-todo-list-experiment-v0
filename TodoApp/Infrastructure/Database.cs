using Dapper;
using Microsoft.Data.Sqlite;

namespace TodoApp.Infrastructure;

public class Database(string connectionString)
{
    public SqliteConnection CreateConnection() => new(connectionString);

    public async Task InitializeAsync()
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS Todos (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                Title       TEXT    NOT NULL,
                IsCompleted INTEGER NOT NULL DEFAULT 0,
                CreatedAt   TEXT    NOT NULL,
                Priority    INTEGER NOT NULL DEFAULT 0
            )
            """);

        // Migration: add IsCompleted column for existing databases
        try
        {
            await conn.ExecuteAsync("ALTER TABLE Todos ADD COLUMN IsCompleted INTEGER NOT NULL DEFAULT 0");
        }
        catch (SqliteException)
        {
            // Column already exists — ignore
        }

        // Migration: add Priority column for existing databases
        try
        {
            await conn.ExecuteAsync("ALTER TABLE Todos ADD COLUMN Priority INTEGER NOT NULL DEFAULT 0");
        }
        catch (SqliteException)
        {
            // Column already exists — ignore
        }

        // Migration: add DueDate column for existing databases
        try
        {
            await conn.ExecuteAsync("ALTER TABLE Todos ADD COLUMN DueDate TEXT NULL");
        }
        catch (SqliteException)
        {
            // Column already exists — ignore
        }

        // Migration: add Notes column for existing databases
        try
        {
            await conn.ExecuteAsync("ALTER TABLE Todos ADD COLUMN Notes TEXT NULL");
        }
        catch (SqliteException)
        {
            // Column already exists — ignore
        }

        // Migration: add IsPinned column for existing databases
        try
        {
            await conn.ExecuteAsync("ALTER TABLE Todos ADD COLUMN IsPinned INTEGER NOT NULL DEFAULT 0");
        }
        catch (SqliteException)
        {
            // Column already exists — ignore
        }

        await conn.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS TodoTags (
                Id      INTEGER PRIMARY KEY AUTOINCREMENT,
                TodoId  INTEGER NOT NULL,
                Name    TEXT    NOT NULL
            )
            """);

        await conn.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS Subtasks (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                TodoId      INTEGER NOT NULL,
                Title       TEXT    NOT NULL,
                IsCompleted INTEGER NOT NULL DEFAULT 0,
                CreatedAt   TEXT    NOT NULL
            )
            """);
    }
}
