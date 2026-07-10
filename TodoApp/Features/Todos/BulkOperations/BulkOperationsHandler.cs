using Dapper;
using TodoApp.Features.Todos;
using TodoApp.Features.Todos.ColorLabel;
using TodoApp.Features.Todos.TimeEstimates;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.BulkOperations;

public class BulkOperationsHandler(Database db)
{
    public async Task CompleteAsync(IReadOnlyList<int> ids)
    {
        if (ids.Count == 0) return;
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Todos SET IsCompleted = 1 WHERE Id IN @Ids",
            new { Ids = ids });
    }

    public async Task DeleteAsync(IReadOnlyList<int> ids)
    {
        if (ids.Count == 0) return;
        using var conn = db.CreateConnection();
        var deletedAt = DateTime.UtcNow.ToString("O");

        await conn.ExecuteAsync("""
            INSERT INTO DeletedTodos (OriginalId, Title, IsCompleted, CreatedAt, Priority, DueDate, IsPinned, Notes, Recurrence, ListId, CompletedAt, TimeEstimate, ColorLabel, IsBlocked, Url, DeletedAt)
            SELECT Id, Title, IsCompleted, CreatedAt, Priority, DueDate, IsPinned, Notes, Recurrence, ListId, CompletedAt, TimeEstimate, ColorLabel, IsBlocked, Url, @DeletedAt
            FROM Todos WHERE Id IN @Ids
            """, new { Ids = ids, DeletedAt = deletedAt });

        await conn.ExecuteAsync(
            "DELETE FROM Todos WHERE Id IN @Ids",
            new { Ids = ids });
    }

    public async Task MoveAsync(IReadOnlyList<int> ids, int targetListId)
    {
        if (ids.Count == 0) return;
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Todos SET ListId = @ListId WHERE Id IN @Ids",
            new { ListId = targetListId, Ids = ids });
    }

    public async Task SetPriorityAsync(IReadOnlyList<int> ids, TodoPriority priority)
    {
        if (ids.Count == 0) return;
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Todos SET Priority = @Priority WHERE Id IN @Ids",
            new { Priority = (int)priority, Ids = ids });
    }

    public async Task SetDueDateAsync(IReadOnlyList<int> ids, DateTime? dueDate)
    {
        if (ids.Count == 0) return;
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Todos SET DueDate = @DueDate WHERE Id IN @Ids",
            new { DueDate = dueDate, Ids = ids });
    }

    public async Task SetTimeEstimateAsync(IReadOnlyList<int> ids, TimeEstimate timeEstimate)
    {
        if (ids.Count == 0) return;
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Todos SET TimeEstimate = @TimeEstimate WHERE Id IN @Ids",
            new { TimeEstimate = (int)timeEstimate, Ids = ids });
    }

    public async Task SetColorLabelAsync(IReadOnlyList<int> ids, TodoColorLabel colorLabel)
    {
        if (ids.Count == 0) return;
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Todos SET ColorLabel = @ColorLabel WHERE Id IN @Ids",
            new { ColorLabel = (int)colorLabel, Ids = ids });
    }

    public async Task AddTagAsync(IReadOnlyList<int> ids, string tagName)
    {
        if (ids.Count == 0) return;
        tagName = tagName.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(tagName))
            throw new ArgumentException("Tag name cannot be empty.", nameof(tagName));

        using var conn = db.CreateConnection();
        foreach (var id in ids)
        {
            var exists = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM TodoTags WHERE TodoId = @TodoId AND Name = @Name",
                new { TodoId = id, Name = tagName });
            if (exists == 0)
                await conn.ExecuteAsync(
                    "INSERT INTO TodoTags (TodoId, Name) VALUES (@TodoId, @Name)",
                    new { TodoId = id, Name = tagName });
        }
    }
}
