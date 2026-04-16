using Dapper;
using Microsoft.Data.Sqlite;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.ReorderTodos;

public class ReorderTodosHandler(Database db)
{
    /// <summary>
    /// Persists a new manual order by assigning SortOrder = position index to each todo id.
    /// </summary>
    public async Task HandleAsync(IReadOnlyList<int> orderedIds)
    {
        if (orderedIds.Count == 0) return;

        using var conn = db.CreateConnection();
        await conn.OpenAsync();
        using var tx = conn.BeginTransaction();

        for (int i = 0; i < orderedIds.Count; i++)
        {
            await conn.ExecuteAsync(
                "UPDATE Todos SET SortOrder = @SortOrder WHERE Id = @Id",
                new { SortOrder = i + 1, Id = orderedIds[i] },
                tx);
        }

        tx.Commit();
    }
}
