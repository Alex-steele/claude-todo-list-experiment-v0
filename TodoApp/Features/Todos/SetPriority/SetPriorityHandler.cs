using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.SetPriority;

public class SetPriorityHandler(Database db)
{
    public async Task HandleAsync(int id, TodoPriority priority)
    {
        using var conn = db.CreateConnection();
        var affected = await conn.ExecuteAsync(
            "UPDATE Todos SET Priority = @Priority WHERE Id = @Id",
            new { Priority = (int)priority, Id = id });

        if (affected == 0)
            throw new ArgumentException($"Todo with id {id} not found.", nameof(id));
    }
}
