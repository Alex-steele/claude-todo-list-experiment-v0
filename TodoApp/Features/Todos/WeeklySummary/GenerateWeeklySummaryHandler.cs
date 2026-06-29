using TodoApp.Features.Todos.GetTodos;

namespace TodoApp.Features.Todos.WeeklySummary;

public class GenerateWeeklySummaryHandler
{
    public string Handle(IReadOnlyList<TodoSummary> todos, DateTime? referenceUtc = null)
    {
        var now = referenceUtc ?? DateTime.UtcNow;
        var weekStart = now.AddDays(-7);

        var completed = todos
            .Where(t => t.IsCompleted && t.CompletedAt.HasValue && t.CompletedAt.Value >= weekStart)
            .OrderByDescending(t => t.CompletedAt)
            .ToList();

        if (completed.Count == 0)
            return "No todos completed in the past 7 days.";

        var lines = completed.Select(t => $"• {t.Title}");
        return $"Completed this week ({completed.Count}):\n{string.Join("\n", lines)}";
    }
}
