using Dapper;
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
