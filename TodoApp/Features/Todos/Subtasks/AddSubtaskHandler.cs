using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.Subtasks;

public class AddSubtaskHandler(Database db)
{
    public async Task<int> HandleAsync(int todoId, string title)
    {
        title = title.Trim();
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Subtask title cannot be empty.", nameof(title));

        using var conn = db.CreateConnection();
        var nextSortOrder = await conn.ExecuteScalarAsync<int>(
            "SELECT COALESCE(MAX(SortOrder), 0) + 1 FROM Subtasks WHERE TodoId = @TodoId",
            new { TodoId = todoId });

        return await conn.ExecuteScalarAsync<int>(
            """
            INSERT INTO Subtasks (TodoId, Title, IsCompleted, CreatedAt, SortOrder)
            VALUES (@TodoId, @Title, 0, @CreatedAt, @SortOrder);
            SELECT last_insert_rowid();
            """,
            new { TodoId = todoId, Title = title, CreatedAt = DateTime.UtcNow.ToString("O"), SortOrder = nextSortOrder });
    }
}
