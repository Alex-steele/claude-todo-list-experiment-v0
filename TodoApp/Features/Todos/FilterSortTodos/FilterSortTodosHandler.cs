using TodoApp.Features.Todos.GetTodos;

namespace TodoApp.Features.Todos.FilterSortTodos;

public class FilterSortTodosHandler
{
    public IReadOnlyList<TodoSummary> Handle(
        IReadOnlyList<TodoSummary> todos,
        TodoStatusFilter statusFilter,
        TodoSortOrder sortOrder,
        string searchQuery = "")
    {
        var searched = string.IsNullOrWhiteSpace(searchQuery)
            ? todos.AsEnumerable()
            : todos.Where(t => t.Title.Contains(searchQuery.Trim(), StringComparison.OrdinalIgnoreCase));

        var filtered = statusFilter switch
        {
            TodoStatusFilter.Active    => searched.Where(t => !t.IsCompleted),
            TodoStatusFilter.Completed => searched.Where(t => t.IsCompleted),
            _                          => searched
        };

        var pinFirst = filtered.OrderByDescending(t => t.IsPinned ? 1 : 0);

        var sorted = sortOrder switch
        {
            TodoSortOrder.Oldest       => pinFirst.ThenBy(t => t.Id),
            TodoSortOrder.DueDateAsc   => pinFirst.ThenBy(t => t.DueDate.HasValue ? 0 : 1)
                                                  .ThenBy(t => t.DueDate)
                                                  .ThenBy(t => -t.Id),
            TodoSortOrder.PriorityDesc => pinFirst.ThenByDescending(t => (int)t.Priority)
                                                  .ThenBy(t => -t.Id),
            _                          => pinFirst.ThenByDescending(t => t.Id)  // Newest (default)
        };

        return sorted.ToList();
    }
}
