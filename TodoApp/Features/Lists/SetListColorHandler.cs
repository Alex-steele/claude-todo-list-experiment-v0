using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Lists;

public class SetListColorHandler(Database db)
{
    public async Task HandleAsync(int id, ListColor color)
    {
        using var conn = db.CreateConnection();
        var affected = await conn.ExecuteAsync(
            "UPDATE TodoLists SET Color = @color WHERE Id = @id",
            new { color = (int)color, id });

        if (affected == 0)
            throw new ArgumentException($"List with id {id} not found.", nameof(id));
    }
}
