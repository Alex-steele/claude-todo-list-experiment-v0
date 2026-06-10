using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.TimeEstimates;

namespace TodoApp.Features.Todos.GetTodosStats;

public record TodoStats(int Total, int Active, int Completed, int Overdue, int CompletionPercentage, int TotalEstimatedMinutes);

public class GetTodosStatsHandler
{
    public TodoStats Handle(IReadOnlyList<TodoSummary> todos)
    {
        var total = todos.Count;
        var completed = todos.Count(t => t.IsCompleted);
        var active = total - completed;
        var today = DateTime.Today;
        var overdue = todos.Count(t => !t.IsCompleted && t.DueDate.HasValue && t.DueDate.Value.Date < today);
        var completionPct = total > 0 ? completed * 100 / total : 0;
        var estimatedMinutes = todos
            .Where(t => !t.IsCompleted && t.TimeEstimate != TimeEstimate.None)
            .Sum(t => (int)t.TimeEstimate);
        return new TodoStats(total, active, completed, overdue, completionPct, estimatedMinutes);
    }
}
