using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.Subtasks;

public class CompleteSubtaskHandler(Database db)
{
    public async Task HandleAsync(int id)
    {
        using var conn = db.CreateConnection();
        var affected = await conn.ExecuteAsync(
            "UPDATE Subtasks SET IsCompleted = 1 - IsCompleted WHERE Id = @Id",
            new { Id = id });

        if (affected == 0)
            throw new ArgumentException($"Subtask {id} not found.", nameof(id));
    }
}
