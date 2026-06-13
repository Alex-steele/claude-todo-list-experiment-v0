using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.DuplicateTodo;

public class DuplicateTodoHandler(Database db)
{
    public async Task<int> HandleAsync(int sourceId)
    {
        using var conn = db.CreateConnection();

        var exists = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Todos WHERE Id = @sourceId",
            new { sourceId });

        if (exists == 0)
            throw new ArgumentException($"Todo with id {sourceId} not found.", nameof(sourceId));

        // Copy all user-authored fields; start fresh on completion state, pin, and sort order.
        var newId = await conn.ExecuteScalarAsync<int>("""
            INSERT INTO Todos
                (Title, CreatedAt, Priority, DueDate, Recurrence, ListId,
                 SortOrder, TimeEstimate, Notes)
            SELECT
                Title || ' (copy)',
                @now,
                Priority,
                DueDate,
                Recurrence,
                ListId,
                (SELECT COALESCE(MAX(SortOrder), 0) + 1 FROM Todos),
                TimeEstimate,
                Notes
            FROM Todos
            WHERE Id = @sourceId;
            SELECT last_insert_rowid();
            """,
            new { sourceId, now = DateTime.UtcNow.ToString("O") });

        return newId;
    }
}
