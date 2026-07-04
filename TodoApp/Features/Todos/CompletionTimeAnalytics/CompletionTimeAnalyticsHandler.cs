using TodoApp.Features.Todos.GetTodos;

namespace TodoApp.Features.Todos.CompletionTimeAnalytics;

public record CompletionTimeResult(
    double? OverallAverageDays,
    double? HighPriorityAverageDays,
    double? MediumPriorityAverageDays,
    double? LowPriorityAverageDays,
    int SampleCount);

public class CompletionTimeAnalyticsHandler
{
    public CompletionTimeResult Handle(IReadOnlyList<TodoSummary> todos)
    {
        var completed = todos
            .Where(t => t.IsCompleted && t.CompletedAt.HasValue)
            .ToList();

        return new CompletionTimeResult(
            Avg(completed),
            Avg(completed.Where(t => t.Priority == TodoPriority.High)),
            Avg(completed.Where(t => t.Priority == TodoPriority.Medium)),
            Avg(completed.Where(t => t.Priority == TodoPriority.Low)),
            completed.Count);
    }

    private static double? Avg(IEnumerable<TodoSummary> items)
    {
        var list = items.ToList();
        if (list.Count == 0) return null;
        var avg = list.Average(t => Math.Max(0, (t.CompletedAt!.Value - t.CreatedAt).TotalDays));
        return Math.Round(avg, 1);
    }
}
