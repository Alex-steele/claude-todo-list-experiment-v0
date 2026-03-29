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
}
