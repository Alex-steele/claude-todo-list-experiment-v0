using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.FilterPresets;

public class DeleteFilterPresetHandler(Database db)
{
    public async Task HandleAsync(int id)
    {
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM FilterPresets WHERE Id = @Id", new { Id = id });
    }
}
