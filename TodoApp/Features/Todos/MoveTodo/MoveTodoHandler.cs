using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.MoveTodo;

public class MoveTodoHandler(Database db)
{
    public async Task HandleAsync(int todoId, int targetListId)
    {
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Todos SET ListId = @targetListId WHERE Id = @todoId",
            new { todoId, targetListId });
    }
}
