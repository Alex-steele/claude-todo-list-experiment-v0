using TodoApp.Features.Todos.GetTodos;

namespace TodoApp.Features.Todos.GetTodosStats;

public record TodoStats(int Total, int Active, int Completed, int Overdue, int CompletionPercentage);

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
        return new TodoStats(total, active, completed, overdue, completionPct);
    }
}
