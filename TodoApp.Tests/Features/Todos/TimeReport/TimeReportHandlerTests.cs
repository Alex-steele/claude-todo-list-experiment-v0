using TodoApp.Features.Todos;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.TimeReport;
using Xunit;

namespace TodoApp.Tests.Features.Todos.TimeReport;

public class TimeReportHandlerTests
{
    private static readonly DateTime Now = new(2026, 7, 28, 12, 0, 0, DateTimeKind.Utc);

    private static TodoSummary MakeTodo(
        int id,
        string title,
        int listId = 1,
        int timeSpentSeconds = 0,
        DateTime? timerStartedAt = null,
        bool isCompleted = false)
        => new(id, title, isCompleted, Now.AddDays(-1), TodoPriority.None, null,
            ListId: listId, TimeSpentSeconds: timeSpentSeconds, TimerStartedAt: timerStartedAt);

    [Fact]
    public void Handle_NoTimeTracked_ReturnsNoData()
    {
        var todos = new[] { MakeTodo(1, "Untracked todo") };

        var result = new TimeReportHandler().Handle(todos, listId: 1, now: Now);

        Assert.False(result.HasData);
        Assert.Equal(0, result.TotalSeconds);
        Assert.Empty(result.Entries);
    }

    [Fact]
    public void Handle_BankedTime_IsIncludedInTotalAndEntries()
    {
        var todos = new[] { MakeTodo(1, "Tracked todo", timeSpentSeconds: 120) };

        var result = new TimeReportHandler().Handle(todos, listId: 1, now: Now);

        Assert.True(result.HasData);
        Assert.Equal(120, result.TotalSeconds);
        var entry = Assert.Single(result.Entries);
        Assert.Equal(1, entry.TodoId);
        Assert.Equal(120, entry.Seconds);
        Assert.False(entry.IsRunning);
    }

    [Fact]
    public void Handle_RunningTimer_AddsElapsedSinceStartAndMarksAsRunning()
    {
        var startedAt = Now.AddSeconds(-90);
        var todos = new[] { MakeTodo(1, "Running todo", timeSpentSeconds: 60, timerStartedAt: startedAt) };

        var result = new TimeReportHandler().Handle(todos, listId: 1, now: Now);

        var entry = Assert.Single(result.Entries);
        Assert.Equal(150, entry.Seconds); // 60 banked + 90 running
        Assert.Equal(150, result.TotalSeconds);
        Assert.True(entry.IsRunning);
    }

    [Fact]
    public void Handle_ScopesToSelectedList()
    {
        var todos = new[]
        {
            MakeTodo(1, "List 1 todo", listId: 1, timeSpentSeconds: 60),
            MakeTodo(2, "List 2 todo", listId: 2, timeSpentSeconds: 300),
        };

        var result = new TimeReportHandler().Handle(todos, listId: 1, now: Now);

        var entry = Assert.Single(result.Entries);
        Assert.Equal(1, entry.TodoId);
        Assert.Equal(60, result.TotalSeconds);
    }

    [Fact]
    public void Handle_MultipleTodos_SortsEntriesByTimeDescending()
    {
        var todos = new[]
        {
            MakeTodo(1, "Short", timeSpentSeconds: 30),
            MakeTodo(2, "Long", timeSpentSeconds: 500),
            MakeTodo(3, "Medium", timeSpentSeconds: 200),
        };

        var result = new TimeReportHandler().Handle(todos, listId: 1, now: Now);

        Assert.Equal(new[] { 2, 3, 1 }, result.Entries.Select(e => e.TodoId));
        Assert.Equal(730, result.TotalSeconds);
    }

    [Fact]
    public void Handle_ZeroTimeTodo_IsExcludedFromEntries()
    {
        var todos = new[]
        {
            MakeTodo(1, "No time", timeSpentSeconds: 0),
            MakeTodo(2, "Some time", timeSpentSeconds: 10),
        };

        var result = new TimeReportHandler().Handle(todos, listId: 1, now: Now);

        var entry = Assert.Single(result.Entries);
        Assert.Equal(2, entry.TodoId);
    }

    [Fact]
    public void Handle_IncludesCompletedTodosThatHaveTrackedTime()
    {
        var todos = new[] { MakeTodo(1, "Done todo", timeSpentSeconds: 45, isCompleted: true) };

        var result = new TimeReportHandler().Handle(todos, listId: 1, now: Now);

        var entry = Assert.Single(result.Entries);
        Assert.True(entry.IsCompleted);
    }
}
