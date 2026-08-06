using TodoApp.Features.Todos.PomodoroTimer;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.PomodoroTimer;

public class GetPomodoroSessionCountsHandlerTests
{
    [Fact]
    public async Task HandleAsync_NoIds_ReturnsEmptyDictionary()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new GetPomodoroSessionCountsHandler(db);

        var result = await handler.HandleAsync([]);

        Assert.Empty(result);
    }

    [Fact]
    public async Task HandleAsync_CountsSessionsPerTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var logHandler = new LogPomodoroSessionHandler(db);
        await logHandler.HandleAsync(1);
        await logHandler.HandleAsync(1);
        await logHandler.HandleAsync(2);

        var handler = new GetPomodoroSessionCountsHandler(db);
        var result = await handler.HandleAsync([1, 2]);

        Assert.Equal(2, result[1]);
        Assert.Equal(1, result[2]);
    }

    [Fact]
    public async Task HandleAsync_IgnoresSessionsWithoutATodoId()
    {
        var db = await TestDatabase.CreateAsync();
        var logHandler = new LogPomodoroSessionHandler(db);
        await logHandler.HandleAsync();
        await logHandler.HandleAsync(1);

        var handler = new GetPomodoroSessionCountsHandler(db);
        var result = await handler.HandleAsync([1]);

        Assert.Equal(1, result[1]);
    }

    [Fact]
    public async Task HandleAsync_TodoWithNoSessions_IsNotPresentInResult()
    {
        var db = await TestDatabase.CreateAsync();
        var logHandler = new LogPomodoroSessionHandler(db);
        await logHandler.HandleAsync(1);

        var handler = new GetPomodoroSessionCountsHandler(db);
        var result = await handler.HandleAsync([1, 2]);

        Assert.True(result.ContainsKey(1));
        Assert.False(result.ContainsKey(2));
    }
}
