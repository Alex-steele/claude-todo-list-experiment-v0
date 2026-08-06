using TodoApp.Features.Todos.GetTodos;

namespace TodoApp.Features.Todos.PriorityMatrix;

public record PriorityMatrixResult(
    IReadOnlyList<TodoSummary> UrgentImportant,
    IReadOnlyList<TodoSummary> ImportantNotUrgent,
    IReadOnlyList<TodoSummary> UrgentNotImportant,
    IReadOnlyList<TodoSummary> NeitherUrgentNorImportant)
{
    public bool HasData =>
        UrgentImportant.Count > 0 || ImportantNotUrgent.Count > 0 ||
        UrgentNotImportant.Count > 0 || NeitherUrgentNorImportant.Count > 0;
}

public class PriorityMatrixHandler
{
    public PriorityMatrixResult Handle(IReadOnlyList<TodoSummary> todos, int listId)
    {
        var today = DateTime.Today;

        var active = todos.Where(t => t.ListId == listId && !t.IsCompleted).ToList();

        bool IsUrgent(TodoSummary t) => t.DueDate.HasValue && t.DueDate.Value.Date <= today;
        bool IsImportant(TodoSummary t) => t.Priority == TodoPriority.High;

        return new PriorityMatrixResult(
            UrgentImportant: active.Where(t => IsUrgent(t) && IsImportant(t)).ToList(),
            ImportantNotUrgent: active.Where(t => !IsUrgent(t) && IsImportant(t)).ToList(),
            UrgentNotImportant: active.Where(t => IsUrgent(t) && !IsImportant(t)).ToList(),
            NeitherUrgentNorImportant: active.Where(t => !IsUrgent(t) && !IsImportant(t)).ToList());
    }
}
