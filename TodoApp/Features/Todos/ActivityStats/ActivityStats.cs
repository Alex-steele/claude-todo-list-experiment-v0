namespace TodoApp.Features.Todos.ActivityStats;

public record DailyCount(DateOnly Date, int Count);

public record ActivityStats(
    int CurrentStreak,
    int LongestStreak,
    int CompletedToday,
    int CompletedThisWeek,
    IReadOnlyList<DailyCount> DailyActivity);
