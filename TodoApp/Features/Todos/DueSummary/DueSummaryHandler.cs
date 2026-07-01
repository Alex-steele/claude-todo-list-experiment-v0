using TodoApp.Features.Todos.GetTodos;

namespace TodoApp.Features.Todos.DueSummary;

public record DueSummaryResult(int Overdue, int DueToday);

public class DueSummaryHandler
{
    public DueSummaryResult Handle(IReadOnlyList<TodoSummary> todos)
    {
        var today = DateTime.Today;
        var overdue  = todos.Count(t => !t.IsCompleted && t.DueDate.HasValue && t.DueDate.Value.Date < today);
        var dueToday = todos.Count(t => !t.IsCompleted && t.DueDate.HasValue && t.DueDate.Value.Date == today);
        return new DueSummaryResult(overdue, dueToday);
    }
}
