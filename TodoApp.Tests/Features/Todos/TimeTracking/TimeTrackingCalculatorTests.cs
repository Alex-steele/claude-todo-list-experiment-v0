using TodoApp.Features.Todos;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.TimeTracking;
using Xunit;

namespace TodoApp.Tests.Features.Todos.TimeTracking;

public class TimeTrackingCalculatorTests
{
    private static TodoSummary MakeTodo(int timeSpentSeconds = 0, DateTime? timerStartedAt = null) =>
        new(1, "Test todo", false, DateTime.UtcNow, TodoPriority.None, null, TimeSpentSeconds: timeSpentSeconds, TimerStartedAt: timerStartedAt);

    [Fact]
    public void GetElapsedSeconds_NoTimerRunning_ReturnsAccumulatedTime()
    {
        var todo = MakeTodo(timeSpentSeconds: 120);

        var elapsed = TimeTrackingCalculator.GetElapsedSeconds(todo, DateTime.UtcNow);

        Assert.Equal(120, elapsed);
    }

    [Fact]
    public void GetElapsedSeconds_TimerRunning_AddsLiveElapsedToAccumulatedTime()
    {
        var now = DateTime.UtcNow;
        var todo = MakeTodo(timeSpentSeconds: 60, timerStartedAt: now.AddSeconds(-30));

        var elapsed = TimeTrackingCalculator.GetElapsedSeconds(todo, now);

        Assert.Equal(90, elapsed);
    }

    [Fact]
    public void GetElapsedSeconds_TimerJustStarted_DoesNotGoNegative()
    {
        var now = DateTime.UtcNow;
        // "now" passed in is slightly before TimerStartedAt due to clock skew in the caller
        var todo = MakeTodo(timeSpentSeconds: 10, timerStartedAt: now.AddSeconds(5));

        var elapsed = TimeTrackingCalculator.GetElapsedSeconds(todo, now);

        Assert.Equal(10, elapsed);
    }

    [Theory]
    [InlineData(0, "0s")]
    [InlineData(45, "45s")]
    [InlineData(59, "59s")]
    [InlineData(60, "1m")]
    [InlineData(150, "2m")]
    [InlineData(3599, "59m")]
    [InlineData(3600, "1h")]
    [InlineData(3660, "1h 1m")]
    [InlineData(7320, "2h 2m")]
    public void FormatDuration_FormatsAsExpected(int totalSeconds, string expected)
    {
        Assert.Equal(expected, TimeTrackingCalculator.FormatDuration(totalSeconds));
    }
}
