using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.Trash;

public class PermanentlyDeleteTrashedTodoHandler(Database db)
{
    public async Task HandleAsync(int trashId)
    {
        using var conn = db.CreateConnection();
        var affected = await conn.ExecuteAsync(
            "DELETE FROM DeletedTodos WHERE TrashId = @TrashId",
            new { TrashId = trashId });

        if (affected == 0)
            throw new ArgumentException($"Trashed todo with id {trashId} not found.");
    }
}
