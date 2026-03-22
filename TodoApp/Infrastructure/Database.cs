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
                Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                Title     TEXT    NOT NULL,
                CreatedAt TEXT    NOT NULL
            )
            """);
    }
}
