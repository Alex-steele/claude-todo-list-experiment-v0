using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.Trash;

public class RestoreTrashedTodoHandler(Database db)
{
    public async Task HandleAsync(int trashId)
    {
        using var conn = db.CreateConnection();

        var restored = await conn.ExecuteAsync("""
            INSERT INTO Todos (Title, IsCompleted, CreatedAt, Priority, DueDate, IsPinned, Notes, Recurrence, ListId, CompletedAt, TimeEstimate, ColorLabel, IsBlocked, Url)
            SELECT Title, IsCompleted, CreatedAt, Priority, DueDate, IsPinned, Notes, Recurrence, ListId, CompletedAt, TimeEstimate, ColorLabel, IsBlocked, Url
            FROM DeletedTodos WHERE TrashId = @TrashId
            """, new { TrashId = trashId });

        if (restored == 0)
            throw new ArgumentException($"Trashed todo with id {trashId} not found.");

        await conn.ExecuteAsync("DELETE FROM DeletedTodos WHERE TrashId = @TrashId", new { TrashId = trashId });
    }
}
