using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.PinTodo;

public class PinTodoHandler(Database db)
{
    public async Task HandleAsync(int id)
    {
        using var conn = db.CreateConnection();
        var affected = await conn.ExecuteAsync(
            "UPDATE Todos SET IsPinned = 1 - IsPinned WHERE Id = @Id",
            new { Id = id });

        if (affected == 0)
            throw new ArgumentException($"Todo {id} not found.", nameof(id));
    }
}
