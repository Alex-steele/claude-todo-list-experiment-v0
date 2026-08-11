using TodoApp.Features.Todos.GetTodos;

namespace TodoApp.Features.Todos.SuggestNext;

public record NextTodoSuggestion(TodoSummary Todo, string Reason);

public class SuggestNextTodoHandler
{
    public NextTodoSuggestion? Handle(IReadOnlyList<TodoSummary> todos, int listId)
    {
        var byId = todos.ToDictionary(t => t.Id);
        var today = DateTime.Today;

        bool IsBlockedByDependency(TodoSummary t) =>
            t.DependsOnTodoId.HasValue
            && byId.TryGetValue(t.DependsOnTodoId.Value, out var dependency)
            && !dependency.IsCompleted;

        var candidates = todos
            .Where(t => t.ListId == listId && !t.IsCompleted && !t.IsBlocked && !IsBlockedByDependency(t))
            .ToList();

        if (candidates.Count == 0) return null;

        bool IsOverdue(TodoSummary t) => t.DueDate.HasValue && t.DueDate.Value.Date < today;
        bool IsDueToday(TodoSummary t) => t.DueDate.HasValue && t.DueDate.Value.Date == today;

        var best = candidates
            .OrderByDescending(IsOverdue)
            .ThenByDescending(IsDueToday)
            .ThenByDescending(t => t.Priority)
            .ThenBy(t => t.DueDate ?? DateTime.MaxValue)
            .ThenBy(t => t.CreatedAt)
            .First();

        var reasons = new List<string>();
        if (IsOverdue(best)) reasons.Add("overdue");
        else if (IsDueToday(best)) reasons.Add("due today");
        if (best.Priority == TodoPriority.High) reasons.Add("high priority");

        var reason = reasons.Count > 0
            ? char.ToUpper(reasons[0][0]) + reasons[0][1..] + (reasons.Count > 1 ? " and " + reasons[1] : "")
            : "the oldest thing on your list";

        return new NextTodoSuggestion(best, reason);
    }
}
