using TodoApp.Features.Todos.GetTodos;

namespace TodoApp.Features.Todos.FocusMode;

public class FocusModeHandler
{
    public IReadOnlyList<TodoSummary> Handle(IReadOnlyList<TodoSummary> todos)
    {
        var today = DateTime.Today;
        return todos
            .Where(t => !t.IsCompleted &&
                        (t.IsPinned ||
                         t.Priority == TodoPriority.High ||
                         (t.DueDate.HasValue && t.DueDate.Value.Date <= today) ||
                         t.Priority == TodoPriority.Medium))
            .OrderBy(t => t.IsPinned ? 0 : 1)
            .ThenBy(t =>
                t.DueDate.HasValue && t.DueDate.Value.Date < today ? 0 :
                t.DueDate.HasValue && t.DueDate.Value.Date == today ? 1 : 2)
            .ThenBy(t => t.Priority switch
            {
                TodoPriority.High   => 0,
                TodoPriority.Medium => 1,
                _                  => 2
            })
            .ToList();
    }
}
