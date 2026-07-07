using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Lists;

public class UnarchiveListHandler(Database db)
{
    public async Task HandleAsync(int listId)
    {
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE TodoLists SET IsArchived = 0 WHERE Id = @Id",
            new { Id = listId });
    }
}
