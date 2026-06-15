using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.Subtasks;

public class EditSubtaskHandler(Database db)
{
    public async Task HandleAsync(int subtaskId, string title)
    {
        title = title.Trim();
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Subtask title cannot be empty.", nameof(title));

        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Subtasks SET Title = @Title WHERE Id = @Id",
            new { Id = subtaskId, Title = title });
    }
}
