using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.CompleteTodo;

public class CompleteTodoHandler(Database db)
{
    public async Task HandleAsync(int id)
    {
        using var conn = db.CreateConnection();

        var row = await conn.QuerySingleOrDefaultAsync<(int IsCompleted, int? DependsOnTodoId)?>(
            "SELECT IsCompleted, DependsOnTodoId FROM Todos WHERE Id = @Id",
            new { Id = id });

        if (row is null)
            throw new ArgumentException($"Todo with id {id} not found.");

        if (row.Value.IsCompleted == 0 && row.Value.DependsOnTodoId is not null)
        {
            var dependencyCompleted = await conn.ExecuteScalarAsync<int>(
                "SELECT IsCompleted FROM Todos WHERE Id = @Id",
                new { Id = row.Value.DependsOnTodoId });
            if (dependencyCompleted == 0)
                throw new ArgumentException("Cannot complete this todo until the todo it depends on is completed.");
        }

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
