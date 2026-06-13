using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.ColorLabel;

public class SetColorLabelHandler(Database db)
{
    public async Task HandleAsync(int id, TodoColorLabel label)
    {
        using var conn = db.CreateConnection();
        var affected = await conn.ExecuteAsync(
            "UPDATE Todos SET ColorLabel = @label WHERE Id = @id",
            new { label = (int)label, id });

        if (affected == 0)
            throw new ArgumentException($"Todo with id {id} not found.", nameof(id));
    }
}
