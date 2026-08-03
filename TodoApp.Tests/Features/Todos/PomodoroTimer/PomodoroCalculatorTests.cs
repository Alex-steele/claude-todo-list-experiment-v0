using TodoApp.Features.Todos.PomodoroTimer;
using Xunit;

namespace TodoApp.Tests.Features.Todos.PomodoroTimer;

public class PomodoroCalculatorTests
{
    [Fact]
    public void GetPhaseDurationSeconds_Work_Returns25Minutes()
    {
        Assert.Equal(25 * 60, PomodoroCalculator.GetPhaseDurationSeconds(PomodoroPhase.Work));
    }

    [Fact]
    public void GetPhaseDurationSeconds_Break_Returns5Minutes()
    {
        Assert.Equal(5 * 60, PomodoroCalculator.GetPhaseDurationSeconds(PomodoroPhase.Break));
    }

    [Fact]
    public void GetRemainingSeconds_AtStart_ReturnsFullDuration()
    {
        var now = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var remaining = PomodoroCalculator.GetRemainingSeconds(PomodoroPhase.Work, now, now);
        Assert.Equal(25 * 60, remaining);
    }

    [Fact]
    public void GetRemainingSeconds_PartwayThrough_ReturnsCorrectRemaining()
    {
        var start = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var now = start.AddSeconds(90);
        var remaining = PomodoroCalculator.GetRemainingSeconds(PomodoroPhase.Work, start, now);
        Assert.Equal(25 * 60 - 90, remaining);
    }

    [Fact]
    public void GetRemainingSeconds_PastDuration_ClampsToZero()
    {
        var start = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var now = start.AddSeconds(25 * 60 + 500);
        var remaining = PomodoroCalculator.GetRemainingSeconds(PomodoroPhase.Work, start, now);
        Assert.Equal(0, remaining);
    }

    [Fact]
    public void IsPhaseComplete_BeforeDurationElapsed_ReturnsFalse()
    {
        var start = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var now = start.AddSeconds(5 * 60 - 1);
        Assert.False(PomodoroCalculator.IsPhaseComplete(PomodoroPhase.Break, start, now));
    }

    [Fact]
    public void IsPhaseComplete_AtOrAfterDurationElapsed_ReturnsTrue()
    {
        var start = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var now = start.AddSeconds(5 * 60);
        Assert.True(PomodoroCalculator.IsPhaseComplete(PomodoroPhase.Break, start, now));
    }

    [Theory]
    [InlineData(0, "0:00")]
    [InlineData(5, "0:05")]
    [InlineData(90, "1:30")]
    [InlineData(1500, "25:00")]
    public void FormatCountdown_FormatsAsMinutesColonSeconds(int totalSeconds, string expected)
    {
        Assert.Equal(expected, PomodoroCalculator.FormatCountdown(totalSeconds));
    }
}
