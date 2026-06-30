using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.TimeEstimates;

namespace TodoApp.Features.Todos.FilterSortTodos;

public class FilterSortTodosHandler
{
    public IReadOnlyList<TodoSummary> Handle(
        IReadOnlyList<TodoSummary> todos,
        TodoStatusFilter statusFilter,
        TodoSortOrder sortOrder,
        string searchQuery = "",
        TodoPriority? priorityFilter = null,
        TodoDateFilter dateFilter = TodoDateFilter.None,
        TodoTimeEstimateFilter timeEstimateFilter = TodoTimeEstimateFilter.Any,
        TodoStalenessFilter stalenessFilter = TodoStalenessFilter.Any)
    {
        var searchTrimmed = searchQuery.Trim();
        var searched = string.IsNullOrWhiteSpace(searchQuery)
            ? todos.AsEnumerable()
            : todos.Where(t => t.Title.Contains(searchTrimmed, StringComparison.OrdinalIgnoreCase)
                            || (t.Notes != null && t.Notes.Contains(searchTrimmed, StringComparison.OrdinalIgnoreCase)));

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

        var timeFiltered = timeEstimateFilter switch
        {
            TodoTimeEstimateFilter.NoEstimate => dateFiltered.Where(t => t.TimeEstimate == TimeEstimate.None),
            TodoTimeEstimateFilter.Max15Min   => dateFiltered.Where(t => t.TimeEstimate != TimeEstimate.None && (int)t.TimeEstimate <= 15),
            TodoTimeEstimateFilter.Max30Min   => dateFiltered.Where(t => t.TimeEstimate != TimeEstimate.None && (int)t.TimeEstimate <= 30),
            TodoTimeEstimateFilter.Max1Hour   => dateFiltered.Where(t => t.TimeEstimate != TimeEstimate.None && (int)t.TimeEstimate <= 60),
            TodoTimeEstimateFilter.Max2Hours  => dateFiltered.Where(t => t.TimeEstimate != TimeEstimate.None && (int)t.TimeEstimate <= 120),
            _                                 => dateFiltered
        };

        var stalenessFiltered = stalenessFilter switch
        {
            TodoStalenessFilter.OneWeekPlus  => timeFiltered.Where(t => !t.IsCompleted && (today - t.CreatedAt.Date).Days >= 7),
            TodoStalenessFilter.TwoWeeksPlus => timeFiltered.Where(t => !t.IsCompleted && (today - t.CreatedAt.Date).Days >= 14),
            TodoStalenessFilter.OneMonthPlus => timeFiltered.Where(t => !t.IsCompleted && (today - t.CreatedAt.Date).Days >= 30),
            _                                => timeFiltered
        };

        var pinFirst = stalenessFiltered.OrderByDescending(t => t.IsPinned ? 1 : 0);

        var sorted = sortOrder switch
        {
            TodoSortOrder.Oldest       => pinFirst.ThenBy(t => t.Id),
            TodoSortOrder.DueDateAsc   => pinFirst.ThenBy(t => t.DueDate.HasValue ? 0 : 1)
                                                  .ThenBy(t => t.DueDate)
                                                  .ThenBy(t => -t.Id),
            TodoSortOrder.PriorityDesc => pinFirst.ThenByDescending(t => (int)t.Priority)
                                                  .ThenBy(t => -t.Id),
            TodoSortOrder.Manual       => stalenessFiltered.AsEnumerable(), // preserve DB order (SortOrder ASC)
            TodoSortOrder.TitleAsc        => pinFirst.ThenBy(t => t.Title, StringComparer.OrdinalIgnoreCase),
            TodoSortOrder.TitleDesc       => pinFirst.ThenByDescending(t => t.Title, StringComparer.OrdinalIgnoreCase),
            TodoSortOrder.TimeEstimateAsc => pinFirst.ThenBy(t => t.TimeEstimate == TimeEstimate.None ? int.MaxValue : (int)t.TimeEstimate)
                                                     .ThenByDescending(t => t.Id),
            TodoSortOrder.TimeEstimateDesc => pinFirst.ThenByDescending(t => (int)t.TimeEstimate)
                                                      .ThenByDescending(t => t.Id),
            _                             => pinFirst.ThenByDescending(t => t.Id)  // Newest (default)
        };

        return sorted.ToList();
    }
}
