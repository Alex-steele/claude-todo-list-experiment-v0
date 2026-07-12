using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.Trash;

public class PurgeExpiredTrashHandler(Database db)
{
    public const int RetentionDays = 30;

    public async Task<int> HandleAsync()
    {
        using var conn = db.CreateConnection();
        var cutoff = DateTime.UtcNow.AddDays(-RetentionDays).ToString("O");

        return await conn.ExecuteAsync(
            "DELETE FROM DeletedTodos WHERE DeletedAt < @Cutoff",
            new { Cutoff = cutoff });
    }
}
