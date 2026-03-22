using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.AddTodo;

public class AddTodoHandler(Database db)
{
    public async Task<int> HandleAsync(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.");

        using var conn = db.CreateConnection();
        var id = await conn.ExecuteScalarAsync<int>("""
            INSERT INTO Todos (Title, CreatedAt)
            VALUES (@Title, @CreatedAt);
            SELECT last_insert_rowid();
            """, new { Title = title.Trim(), CreatedAt = DateTime.UtcNow.ToString("O") });

        return id;
    }
}
