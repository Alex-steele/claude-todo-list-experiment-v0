using System.Globalization;
using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.TimeTracking;

public class StartTimerHandler(Database db)
{
    public async Task HandleAsync(int todoId)
    {
        using var conn = db.CreateConnection();
        var now = DateTime.UtcNow;

        // Only one timer runs at a time — stop any other todo's running timer first.
        var otherRunning = await conn.QueryAsync<(int Id, string TimerStartedAt)>(
            "SELECT Id, TimerStartedAt FROM Todos WHERE TimerStartedAt IS NOT NULL AND Id != @Id",
            new { Id = todoId });

        foreach (var (id, timerStartedAt) in otherRunning)
        {
            // DateTimeStyles.RoundtripKind is required so a "Z"-suffixed UTC string parses
            // back with Kind=Utc instead of being silently converted to local time.
            var elapsed = Math.Max(0, (int)(now - DateTime.Parse(timerStartedAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)).TotalSeconds);
            await conn.ExecuteAsync(
                "UPDATE Todos SET TimeSpentSeconds = TimeSpentSeconds + @Elapsed, TimerStartedAt = NULL WHERE Id = @Id",
                new { Elapsed = elapsed, Id = id });
        }

        var affected = await conn.ExecuteAsync(
            "UPDATE Todos SET TimerStartedAt = @Now WHERE Id = @Id",
            new { Now = now.ToString("O"), Id = todoId });

        if (affected == 0)
            throw new ArgumentException($"Todo with id {todoId} not found.");
    }
}
