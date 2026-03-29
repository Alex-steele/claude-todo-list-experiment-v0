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

        var sorted = sortOrder switch
        {
            TodoSortOrder.Oldest       => filtered.OrderBy(t => t.Id),
            TodoSortOrder.DueDateAsc   => filtered.OrderBy(t => t.DueDate.HasValue ? 0 : 1)
                                                  .ThenBy(t => t.DueDate)
                                                  .ThenBy(t => -t.Id),
            TodoSortOrder.PriorityDesc => filtered.OrderByDescending(t => (int)t.Priority)
                                                  .ThenBy(t => -t.Id),
            _                          => filtered.OrderByDescending(t => t.Id)  // Newest (default)
        };

        return sorted.ToList();
    }
}
