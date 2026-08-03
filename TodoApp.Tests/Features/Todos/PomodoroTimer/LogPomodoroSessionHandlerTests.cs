using Dapper;
using TodoApp.Features.Todos.PomodoroTimer;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.PomodoroTimer;

public class LogPomodoroSessionHandlerTests
{
    [Fact]
    public async Task HandleAsync_InsertsSessionRowWithRecentTimestamp()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new LogPomodoroSessionHandler(db);

        await handler.HandleAsync();

        using var conn = db.CreateConnection();
        var completedAtRaw = await conn.QuerySingleAsync<string>("SELECT CompletedAt FROM PomodoroSessions");
        var completedAt = DateTime.Parse(completedAtRaw, null, System.Globalization.DateTimeStyles.RoundtripKind);

        Assert.True((DateTime.UtcNow - completedAt).TotalSeconds < 5);
    }

    [Fact]
    public async Task HandleAsync_CalledMultipleTimes_InsertsOneRowPerCall()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new LogPomodoroSessionHandler(db);

        await handler.HandleAsync();
        await handler.HandleAsync();
        await handler.HandleAsync();

        using var conn = db.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM PomodoroSessions");
        Assert.Equal(3, count);
    }
}
