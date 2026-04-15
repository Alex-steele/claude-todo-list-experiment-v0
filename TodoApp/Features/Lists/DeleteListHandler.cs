using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Lists;

public class DeleteListHandler(Database db)
{
    public const int DefaultListId = 1;

    public async Task HandleAsync(int id)
    {
        if (id == DefaultListId)
            throw new InvalidOperationException("Cannot delete the default list.");

        using var conn = db.CreateConnection();
        // Move todos from deleted list to the default list
        await conn.ExecuteAsync(
            "UPDATE Todos SET ListId = @DefaultId WHERE ListId = @Id",
            new { DefaultId = DefaultListId, Id = id });

        await conn.ExecuteAsync("DELETE FROM TodoLists WHERE Id = @Id", new { Id = id });
    }
}
