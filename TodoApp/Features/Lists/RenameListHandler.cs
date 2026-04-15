using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Lists;

public class RenameListHandler(Database db)
{
    public async Task HandleAsync(int id, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("List name cannot be empty.");

        using var conn = db.CreateConnection();
        var affected = await conn.ExecuteAsync(
            "UPDATE TodoLists SET Name = @Name WHERE Id = @Id",
            new { Name = newName.Trim(), Id = id });

        if (affected == 0)
            throw new ArgumentException($"List with id {id} not found.");
    }
}
