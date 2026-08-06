using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.PomodoroTimer;

public class GetPomodoroSessionCountsHandler(Database db)
{
    public async Task<Dictionary<int, int>> HandleAsync(IEnumerable<int> todoIds)
    {
        var ids = todoIds.ToList();
        if (ids.Count == 0) return [];

        using var conn = db.CreateConnection();
        var rows = await conn.QueryAsync<(int TodoId, int Count)>(
            "SELECT TodoId, COUNT(*) AS Count FROM PomodoroSessions WHERE TodoId IN @Ids GROUP BY TodoId",
            new { Ids = ids });

        return rows.ToDictionary(r => r.TodoId, r => r.Count);
    }
}
