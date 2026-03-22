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
                CreatedAt   TEXT    NOT NULL
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
    }
}
