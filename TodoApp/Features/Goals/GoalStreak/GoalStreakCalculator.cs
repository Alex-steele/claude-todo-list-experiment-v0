using TodoApp.Features.Todos.ActivityStats;

namespace TodoApp.Features.Goals.GoalStreak;

public static class GoalStreakCalculator
{
    public static int Calculate(IReadOnlyList<DailyCount> dailyActivity, int? target, DateOnly today)
    {
        if (target is not > 0) return 0;

        var byDate = dailyActivity.ToDictionary(d => d.Date, d => d.Count);

        var cursor = byDate.TryGetValue(today, out var todayCount) && todayCount >= target
            ? today
            : today.AddDays(-1);

        var streak = 0;
        while (byDate.TryGetValue(cursor, out var count) && count >= target)
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }

        return streak;
    }
}
