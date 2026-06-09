using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.CompleteTodo;

public class CompleteTodoHandler(Database db)
{
    public async Task HandleAsync(int id)
    {
        using var conn = db.CreateConnection();
        var affected = await conn.ExecuteAsync(
            """
            UPDATE Todos
            SET IsCompleted = 1 - IsCompleted,
                CompletedAt = CASE WHEN IsCompleted = 0 THEN @now ELSE NULL END
            WHERE Id = @Id
            """,
            new { Id = id, now = DateTime.UtcNow.ToString("O") });

        if (affected == 0)
            throw new ArgumentException($"Todo with id {id} not found.");
    }
}
