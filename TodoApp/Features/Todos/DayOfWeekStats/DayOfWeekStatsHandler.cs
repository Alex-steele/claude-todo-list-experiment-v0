using TodoApp.Features.Todos.GetTodos;

namespace TodoApp.Features.Todos.DayOfWeekStats;

public record DayOfWeekCount(DayOfWeek Day, int Count);

public record DayOfWeekStatsResult(IReadOnlyList<DayOfWeekCount> Counts, DayOfWeek? BestDay)
{
    public bool HasData => Counts.Sum(c => c.Count) > 0;
}

public class DayOfWeekStatsHandler
{
    public DayOfWeekStatsResult Handle(IReadOnlyList<TodoSummary> todos, int listId)
    {
        var counts = new int[7];
        foreach (var todo in todos)
        {
            if (todo.ListId != listId || !todo.IsCompleted || todo.CompletedAt is not { } completedAt)
                continue;

            counts[(int)completedAt.DayOfWeek]++;
        }

        var result = Enumerable.Range(0, 7)
            .Select(i => new DayOfWeekCount((DayOfWeek)i, counts[i]))
            .ToList();

        var max = result.Max(c => c.Count);
        DayOfWeek? bestDay = max > 0 ? result.First(c => c.Count == max).Day : null;

        return new DayOfWeekStatsResult(result, bestDay);
    }
}
