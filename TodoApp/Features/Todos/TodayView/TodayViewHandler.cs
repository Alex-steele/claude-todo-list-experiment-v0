using TodoApp.Features.Lists;
using TodoApp.Features.Todos.GetTodos;

namespace TodoApp.Features.Todos.TodayView;

public record TodayListGroup(int ListId, string ListName, IReadOnlyList<TodoSummary> Todos);

public class TodayViewHandler
{
    public IReadOnlyList<TodayListGroup> Handle(
        IReadOnlyList<TodoSummary> allTodos,
        IReadOnlyList<TodoList> allLists)
    {
        var today = DateTime.Today;
        var listMap = allLists.ToDictionary(l => l.Id, l => l.Name);

        var urgent = allTodos
            .Where(t => !t.IsCompleted && t.DueDate.HasValue && t.DueDate.Value.Date <= today)
            .GroupBy(t => t.ListId)
            .Select(g =>
            {
                var listName = listMap.TryGetValue(g.Key, out var name) ? name : $"List {g.Key}";
                var sorted = g
                    .OrderBy(t => t.DueDate!.Value.Date)
                    .ThenBy(t => t.Priority switch
                    {
                        TodoPriority.High   => 0,
                        TodoPriority.Medium => 1,
                        TodoPriority.Low    => 2,
                        _                  => 3
                    })
                    .ToList();
                return new TodayListGroup(g.Key, listName, sorted);
            })
            .OrderByDescending(g => g.Todos.Count)
            .ToList();

        return urgent;
    }

    public int CountUrgent(IReadOnlyList<TodoSummary> allTodos)
    {
        var today = DateTime.Today;
        return allTodos.Count(t => !t.IsCompleted && t.DueDate.HasValue && t.DueDate.Value.Date <= today);
    }
}
