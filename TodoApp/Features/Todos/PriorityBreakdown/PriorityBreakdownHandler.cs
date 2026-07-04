using TodoApp.Features.Todos.GetTodos;

namespace TodoApp.Features.Todos.PriorityBreakdown;

public record PriorityStats(int Active, int Completed)
{
    public int Total => Active + Completed;
    public int CompletionPercent => Total == 0 ? 0 : Completed * 100 / Total;
}

public record PriorityBreakdownResult(PriorityStats High, PriorityStats Medium, PriorityStats Low)
{
    public bool HasData => High.Total > 0 || Medium.Total > 0 || Low.Total > 0;
}

public class PriorityBreakdownHandler
{
    public PriorityBreakdownResult Handle(IReadOnlyList<TodoSummary> todos)
    {
        return new PriorityBreakdownResult(
            Compute(todos, TodoPriority.High),
            Compute(todos, TodoPriority.Medium),
            Compute(todos, TodoPriority.Low));
    }

    private static PriorityStats Compute(IReadOnlyList<TodoSummary> todos, TodoPriority priority)
    {
        var subset = todos.Where(t => t.Priority == priority).ToList();
        var completed = subset.Count(t => t.IsCompleted);
        return new PriorityStats(subset.Count - completed, completed);
    }
}
