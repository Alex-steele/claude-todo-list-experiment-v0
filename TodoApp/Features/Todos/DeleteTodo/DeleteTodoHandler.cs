using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.DeleteTodo;

public class DeleteTodoHandler(Database db)
{
    public async Task HandleAsync(int id)
    {
        using var conn = db.CreateConnection();
        var deletedAt = DateTime.UtcNow.ToString("O");

        var snapshotted = await conn.ExecuteAsync("""
            INSERT INTO DeletedTodos (OriginalId, Title, IsCompleted, CreatedAt, Priority, DueDate, IsPinned, Notes, Recurrence, ListId, CompletedAt, TimeEstimate, ColorLabel, IsBlocked, Url, DeletedAt)
            SELECT Id, Title, IsCompleted, CreatedAt, Priority, DueDate, IsPinned, Notes, Recurrence, ListId, CompletedAt, TimeEstimate, ColorLabel, IsBlocked, Url, @DeletedAt
            FROM Todos WHERE Id = @Id
            """, new { Id = id, DeletedAt = deletedAt });

        if (snapshotted == 0)
            throw new ArgumentException($"Todo with id {id} not found.");

        await conn.ExecuteAsync("DELETE FROM Todos WHERE Id = @Id", new { Id = id });
    }
}
