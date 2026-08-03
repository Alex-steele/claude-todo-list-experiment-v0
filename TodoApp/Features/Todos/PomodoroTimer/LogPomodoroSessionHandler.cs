using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.PomodoroTimer;

public class LogPomodoroSessionHandler(Database db)
{
    public async Task HandleAsync()
    {
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            "INSERT INTO PomodoroSessions (CompletedAt) VALUES (@CompletedAt)",
            new { CompletedAt = DateTime.UtcNow.ToString("O") });
    }
}
