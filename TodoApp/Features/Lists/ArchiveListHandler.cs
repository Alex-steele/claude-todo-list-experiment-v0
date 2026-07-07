using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Lists;

public class ArchiveListHandler(Database db)
{
    public const int DefaultListId = 1;

    public async Task HandleAsync(int listId)
    {
        if (listId == DefaultListId)
            throw new InvalidOperationException("The default list cannot be archived.");

        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE TodoLists SET IsArchived = 1 WHERE Id = @Id",
            new { Id = listId });
    }
}
