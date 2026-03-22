using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.CompleteTodo;

public class CompleteTodoHandler(Database db)
{
    public async Task HandleAsync(int id)
    {
        using var conn = db.CreateConnection();
        var affected = await conn.ExecuteAsync(
            "UPDATE Todos SET IsCompleted = 1 - IsCompleted WHERE Id = @Id",
            new { Id = id });

        if (affected == 0)
            throw new ArgumentException($"Todo with id {id} not found.");
    }
}
