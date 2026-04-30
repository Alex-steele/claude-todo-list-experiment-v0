using TodoApp.Features.Todos.GetTodos;

namespace TodoApp.Features.Todos.FilterSortTodos;

public class FilterSortTodosHandler
{
    public IReadOnlyList<TodoSummary> Handle(
        IReadOnlyList<TodoSummary> todos,
        TodoStatusFilter statusFilter,
        TodoSortOrder sortOrder,
        string searchQuery = "",
        TodoPriority? priorityFilter = null,
        TodoDateFilter dateFilter = TodoDateFilter.None)
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

        var priorityFiltered = priorityFilter.HasValue
            ? filtered.Where(t => t.Priority == priorityFilter.Value)
            : filtered;

        var today = DateTime.Today;
        var endOfWeek = today.AddDays(7);

        var dateFiltered = dateFilter switch
        {
            TodoDateFilter.Overdue      => priorityFiltered.Where(t => !t.IsCompleted && t.DueDate.HasValue && t.DueDate.Value.Date < today),
            TodoDateFilter.DueToday     => priorityFiltered.Where(t => !t.IsCompleted && t.DueDate.HasValue && t.DueDate.Value.Date == today),
            TodoDateFilter.DueThisWeek  => priorityFiltered.Where(t => !t.IsCompleted && t.DueDate.HasValue && t.DueDate.Value.Date >= today && t.DueDate.Value.Date < endOfWeek),
            _                           => priorityFiltered
        };

        var pinFirst = dateFiltered.OrderByDescending(t => t.IsPinned ? 1 : 0);

        var sorted = sortOrder switch
        {
            TodoSortOrder.Oldest       => pinFirst.ThenBy(t => t.Id),
            TodoSortOrder.DueDateAsc   => pinFirst.ThenBy(t => t.DueDate.HasValue ? 0 : 1)
                                                  .ThenBy(t => t.DueDate)
                                                  .ThenBy(t => -t.Id),
            TodoSortOrder.PriorityDesc => pinFirst.ThenByDescending(t => (int)t.Priority)
                                                  .ThenBy(t => -t.Id),
            TodoSortOrder.Manual       => dateFiltered.AsEnumerable(), // preserve DB order (SortOrder ASC)
            _                          => pinFirst.ThenByDescending(t => t.Id)  // Newest (default)
        };

        return sorted.ToList();
    }
}
