using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.PomodoroTimer;

public class LogPomodoroSessionHandler(Database db)
{
    public async Task HandleAsync(int? todoId = null)
    {
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            "INSERT INTO PomodoroSessions (CompletedAt, TodoId) VALUES (@CompletedAt, @TodoId)",
            new { CompletedAt = DateTime.UtcNow.ToString("O"), TodoId = todoId });
    }
}
