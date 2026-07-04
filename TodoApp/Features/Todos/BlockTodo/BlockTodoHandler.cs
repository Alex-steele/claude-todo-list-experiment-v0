using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.BlockTodo;

public class BlockTodoHandler(Database db)
{
    public async Task HandleAsync(int id)
    {
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Todos SET IsBlocked = CASE WHEN IsBlocked = 1 THEN 0 ELSE 1 END WHERE Id = @id",
            new { id });
    }
}
