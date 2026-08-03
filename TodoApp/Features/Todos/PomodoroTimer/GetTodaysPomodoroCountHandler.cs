using System.Globalization;
using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.PomodoroTimer;

public class GetTodaysPomodoroCountHandler(Database db)
{
    public async Task<int> HandleAsync(DateTime? referenceUtc = null)
    {
        using var conn = db.CreateConnection();
        var rows = await conn.QueryAsync<string>("SELECT CompletedAt FROM PomodoroSessions");

        var today = (referenceUtc ?? DateTime.UtcNow).Date;
        return rows.Count(r =>
            DateTime.Parse(r, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).Date == today);
    }
}
