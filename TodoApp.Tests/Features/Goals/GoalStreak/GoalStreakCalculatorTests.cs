using TodoApp.Features.Goals.GoalStreak;
using TodoApp.Features.Todos.ActivityStats;
using Xunit;

namespace TodoApp.Tests.Features.Goals.GoalStreak;

public class GoalStreakCalculatorTests
{
    private static readonly DateOnly Today = new(2026, 8, 4);

    [Fact]
    public void Calculate_NoGoalSet_ReturnsZero()
    {
        var activity = new List<DailyCount> { new(Today, 5) };

        var streak = GoalStreakCalculator.Calculate(activity, null, Today);

        Assert.Equal(0, streak);
    }

    [Fact]
    public void Calculate_GoalOfZero_ReturnsZero()
    {
        var activity = new List<DailyCount> { new(Today, 5) };

        var streak = GoalStreakCalculator.Calculate(activity, 0, Today);

        Assert.Equal(0, streak);
    }

    [Fact]
    public void Calculate_TodayMeetsGoal_ReturnsOne()
    {
        var activity = new List<DailyCount> { new(Today, 3) };

        var streak = GoalStreakCalculator.Calculate(activity, 3, Today);

        Assert.Equal(1, streak);
    }

    [Fact]
    public void Calculate_TodayExceedsGoal_StillCounts()
    {
        var activity = new List<DailyCount> { new(Today, 10) };

        var streak = GoalStreakCalculator.Calculate(activity, 3, Today);

        Assert.Equal(1, streak);
    }

    [Fact]
    public void Calculate_TodayBelowGoal_ButYesterdayMet_CountsFromYesterday()
    {
        var activity = new List<DailyCount>
        {
            new(Today.AddDays(-1), 5),
            new(Today, 1),
        };

        var streak = GoalStreakCalculator.Calculate(activity, 3, Today);

        Assert.Equal(1, streak);
    }

    [Fact]
    public void Calculate_TodayBelowGoal_AndYesterdayBelowGoal_ReturnsZero()
    {
        var activity = new List<DailyCount>
        {
            new(Today.AddDays(-1), 1),
            new(Today, 1),
        };

        var streak = GoalStreakCalculator.Calculate(activity, 3, Today);

        Assert.Equal(0, streak);
    }

    [Fact]
    public void Calculate_ConsecutiveDaysMeetingGoal_CountsAll()
    {
        var activity = new List<DailyCount>
        {
            new(Today.AddDays(-3), 5),
            new(Today.AddDays(-2), 4),
            new(Today.AddDays(-1), 3),
            new(Today, 6),
        };

        var streak = GoalStreakCalculator.Calculate(activity, 3, Today);

        Assert.Equal(4, streak);
    }

    [Fact]
    public void Calculate_GapInHistory_StopsAtGap()
    {
        var activity = new List<DailyCount>
        {
            new(Today.AddDays(-3), 5),
            new(Today.AddDays(-2), 1), // below goal — breaks the streak
            new(Today.AddDays(-1), 3),
            new(Today, 6),
        };

        var streak = GoalStreakCalculator.Calculate(activity, 3, Today);

        Assert.Equal(2, streak);
    }

    [Fact]
    public void Calculate_MissingDayInWindow_TreatedAsBreak()
    {
        // Day at Today-2 has no entry at all (outside the fetched window)
        var activity = new List<DailyCount>
        {
            new(Today.AddDays(-1), 3),
            new(Today, 6),
        };

        var streak = GoalStreakCalculator.Calculate(activity, 3, Today);

        Assert.Equal(2, streak);
    }

    [Fact]
    public void Calculate_EmptyActivity_ReturnsZero()
    {
        var streak = GoalStreakCalculator.Calculate([], 3, Today);

        Assert.Equal(0, streak);
    }
}
