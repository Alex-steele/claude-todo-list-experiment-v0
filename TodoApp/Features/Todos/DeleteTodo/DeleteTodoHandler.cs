using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.DeleteTodo;

public class DeleteTodoHandler(Database db)
{
    public async Task HandleAsync(int id)
    {
        using var conn = db.CreateConnection();
        var affected = await conn.ExecuteAsync(
            "DELETE FROM Todos WHERE Id = @Id",
            new { Id = id });

        if (affected == 0)
            throw new ArgumentException($"Todo with id {id} not found.");
    }
}
