using System.Globalization;
using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.TimeTracking;

public class StopTimerHandler(Database db)
{
    public async Task HandleAsync(int todoId)
    {
        using var conn = db.CreateConnection();
        var timerStartedAt = await conn.ExecuteScalarAsync<string?>(
            "SELECT TimerStartedAt FROM Todos WHERE Id = @Id", new { Id = todoId });

        if (timerStartedAt is null)
            return; // no active timer — nothing to stop

        // DateTimeStyles.RoundtripKind is required so a "Z"-suffixed UTC string parses
        // back with Kind=Utc instead of being silently converted to local time.
        var elapsed = Math.Max(0, (int)(DateTime.UtcNow - DateTime.Parse(timerStartedAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)).TotalSeconds);
        await conn.ExecuteAsync(
            "UPDATE Todos SET TimeSpentSeconds = TimeSpentSeconds + @Elapsed, TimerStartedAt = NULL WHERE Id = @Id",
            new { Elapsed = elapsed, Id = todoId });
    }
}
