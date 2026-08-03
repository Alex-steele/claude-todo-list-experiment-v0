using Dapper;
using TodoApp.Features.Todos.PomodoroTimer;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.PomodoroTimer;

public class GetTodaysPomodoroCountHandlerTests
{
    private static async Task InsertSessionAsync(TodoApp.Infrastructure.Database db, DateTime completedAtUtc)
    {
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            "INSERT INTO PomodoroSessions (CompletedAt) VALUES (@CompletedAt)",
            new { CompletedAt = completedAtUtc.ToString("O") });
    }

    [Fact]
    public async Task HandleAsync_NoSessions_ReturnsZero()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new GetTodaysPomodoroCountHandler(db);

        var count = await handler.HandleAsync();

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task HandleAsync_SessionsCompletedToday_AreCounted()
    {
        var db = await TestDatabase.CreateAsync();
        var reference = new DateTime(2026, 6, 15, 14, 0, 0, DateTimeKind.Utc);
        await InsertSessionAsync(db, reference.AddHours(-2));
        await InsertSessionAsync(db, reference.AddHours(-6));

        var handler = new GetTodaysPomodoroCountHandler(db);
        var count = await handler.HandleAsync(reference);

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task HandleAsync_SessionsFromPreviousDay_AreExcluded()
    {
        var db = await TestDatabase.CreateAsync();
        var reference = new DateTime(2026, 6, 15, 1, 0, 0, DateTimeKind.Utc);
        await InsertSessionAsync(db, reference.AddHours(-2)); // previous day
        await InsertSessionAsync(db, reference); // today

        var handler = new GetTodaysPomodoroCountHandler(db);
        var count = await handler.HandleAsync(reference);

        Assert.Equal(1, count);
    }
}
