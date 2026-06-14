using TodoApp.Features.Todos.ActivityStats;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.CompleteTodo;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.ActivityStats;

public class ActivityStatsHandlerTests
{
    // --- ComputeStreaks unit tests (pure logic, no DB) ---

    [Fact]
    public void ComputeStreaks_EmptySet_ReturnsZero()
    {
        var (current, longest) = ActivityStatsHandler.ComputeStreaks([], DateTime.Today);
        Assert.Equal(0, current);
        Assert.Equal(0, longest);
    }

    [Fact]
    public void ComputeStreaks_OnlyToday_Returns1()
    {
        var today = DateTime.Today;
        var (current, longest) = ActivityStatsHandler.ComputeStreaks([today], today);
        Assert.Equal(1, current);
        Assert.Equal(1, longest);
    }

    [Fact]
    public void ComputeStreaks_ThreeConsecutiveDaysEndingToday_Returns3()
    {
        var today = DateTime.Today;
        var dates = new HashSet<DateTime> { today, today.AddDays(-1), today.AddDays(-2) };
        var (current, longest) = ActivityStatsHandler.ComputeStreaks(dates, today);
        Assert.Equal(3, current);
        Assert.Equal(3, longest);
    }

    [Fact]
    public void ComputeStreaks_GapInStreak_CurrentCountsFromMostRecent()
    {
        var today = DateTime.Today;
        // today and yesterday → streak 2, but 3 days ago is a gap then 5 days ago
        var dates = new HashSet<DateTime> { today, today.AddDays(-1), today.AddDays(-4) };
        var (current, longest) = ActivityStatsHandler.ComputeStreaks(dates, today);
        Assert.Equal(2, current);
        Assert.Equal(2, longest);
    }

    [Fact]
    public void ComputeStreaks_LongestStreakInPast_IsCorrect()
    {
        var today = DateTime.Today;
        // Past 5-day run, but only today active now
        var dates = new HashSet<DateTime>
        {
            today,
            today.AddDays(-10),
            today.AddDays(-11),
            today.AddDays(-12),
            today.AddDays(-13),
            today.AddDays(-14)
        };
        var (current, longest) = ActivityStatsHandler.ComputeStreaks(dates, today);
        Assert.Equal(1, current);
        Assert.Equal(5, longest);
    }

    [Fact]
    public void ComputeStreaks_NothingToday_StreakCountsFromYesterday()
    {
        var today = DateTime.Today;
        var yesterday = today.AddDays(-1);
        var dates = new HashSet<DateTime> { yesterday, today.AddDays(-2) };
        var (current, longest) = ActivityStatsHandler.ComputeStreaks(dates, today);
        Assert.Equal(2, current);
        Assert.Equal(2, longest);
    }

    [Fact]
    public void ComputeStreaks_NothingTodayOrYesterday_CurrentIsZero()
    {
        var today = DateTime.Today;
        var dates = new HashSet<DateTime> { today.AddDays(-2), today.AddDays(-3) };
        var (current, longest) = ActivityStatsHandler.ComputeStreaks(dates, today);
        Assert.Equal(0, current);
        Assert.Equal(2, longest);
    }

    // --- Integration tests with real DB ---

    [Fact]
    public async Task HandleAsync_NoCompletions_ReturnsZeroStats()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Not completed todo");

        var handler = new ActivityStatsHandler(db);
        var stats = await handler.HandleAsync();

        Assert.Equal(0, stats.CurrentStreak);
        Assert.Equal(0, stats.LongestStreak);
        Assert.Equal(0, stats.CompletedToday);
        Assert.Equal(0, stats.CompletedThisWeek);
    }

    [Fact]
    public async Task HandleAsync_CompletedTodoToday_CountsInTodayAndWeek()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var completeHandler = new CompleteTodoHandler(db);

        var id = await addHandler.HandleAsync("Buy milk");
        await completeHandler.HandleAsync(id);

        var handler = new ActivityStatsHandler(db);
        var stats = await handler.HandleAsync();

        Assert.Equal(1, stats.CompletedToday);
        Assert.Equal(1, stats.CompletedThisWeek);
        Assert.Equal(1, stats.CurrentStreak);
        Assert.Equal(1, stats.LongestStreak);
    }

    [Fact]
    public async Task HandleAsync_MultipleCompletedTodosToday_CountsAll()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var completeHandler = new CompleteTodoHandler(db);

        var id1 = await addHandler.HandleAsync("Task A");
        var id2 = await addHandler.HandleAsync("Task B");
        var id3 = await addHandler.HandleAsync("Task C");
        await completeHandler.HandleAsync(id1);
        await completeHandler.HandleAsync(id2);
        await completeHandler.HandleAsync(id3);

        var handler = new ActivityStatsHandler(db);
        var stats = await handler.HandleAsync();

        Assert.Equal(3, stats.CompletedToday);
        Assert.Equal(3, stats.CompletedThisWeek);
    }

    [Fact]
    public async Task HandleAsync_ActiveTodosNotCounted()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Incomplete task");

        var handler = new ActivityStatsHandler(db);
        var stats = await handler.HandleAsync();

        Assert.Equal(0, stats.CompletedToday);
        Assert.Equal(0, stats.CompletedThisWeek);
        Assert.Equal(0, stats.CurrentStreak);
    }

    // --- DailyActivity heatmap tests ---

    [Fact]
    public async Task HandleAsync_DailyActivity_AlwaysContains14Entries()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new ActivityStatsHandler(db);

        var stats = await handler.HandleAsync();

        Assert.Equal(14, stats.DailyActivity.Count);
    }

    [Fact]
    public async Task HandleAsync_DailyActivity_OldestDayIs13DaysAgo()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new ActivityStatsHandler(db);

        var stats = await handler.HandleAsync();
        var oldest = stats.DailyActivity[0].Date;
        var expected = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-13));

        Assert.Equal(expected, oldest);
    }

    [Fact]
    public async Task HandleAsync_DailyActivity_NewestDayIsToday()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new ActivityStatsHandler(db);

        var stats = await handler.HandleAsync();
        var newest = stats.DailyActivity[13].Date;
        var expected = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        Assert.Equal(expected, newest);
    }

    [Fact]
    public async Task HandleAsync_DailyActivity_TodayCountMatchesCompletedToday()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var completeHandler = new CompleteTodoHandler(db);

        var id1 = await addHandler.HandleAsync("Task A");
        var id2 = await addHandler.HandleAsync("Task B");
        await completeHandler.HandleAsync(id1);
        await completeHandler.HandleAsync(id2);

        var handler = new ActivityStatsHandler(db);
        var stats = await handler.HandleAsync();

        var todayEntry = stats.DailyActivity.Last();
        Assert.Equal(stats.CompletedToday, todayEntry.Count);
    }

    [Fact]
    public async Task HandleAsync_DailyActivity_NoCompletions_AllCountsAreZero()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new ActivityStatsHandler(db);

        var stats = await handler.HandleAsync();

        Assert.All(stats.DailyActivity, d => Assert.Equal(0, d.Count));
    }
}
