using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.Tags;

public class RemoveTagHandler(Database db)
{
    public async Task HandleAsync(int tagId)
    {
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM TodoTags WHERE Id = @Id", new { Id = tagId });
    }
}
