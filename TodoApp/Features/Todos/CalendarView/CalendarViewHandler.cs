using TodoApp.Features.Todos.GetTodos;

namespace TodoApp.Features.Todos.CalendarView;

public record CalendarDay(DateTime Date, IReadOnlyList<TodoSummary> Todos);

public class CalendarViewHandler
{
    public IReadOnlyList<CalendarDay> Handle(IReadOnlyList<TodoSummary> todos, int listId, int year, int month)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);

        var byDay = todos
            .Where(t => t.ListId == listId && t.DueDate.HasValue && t.DueDate.Value.Date >= start && t.DueDate.Value.Date < end)
            .GroupBy(t => t.DueDate!.Value.Date)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<TodoSummary>)g
                .OrderBy(t => t.IsCompleted)
                .ThenBy(t => t.Priority switch
                {
                    TodoPriority.High => 0,
                    TodoPriority.Medium => 1,
                    TodoPriority.Low => 2,
                    _ => 3
                })
                .ThenBy(t => t.Title)
                .ToList());

        var days = new List<CalendarDay>();
        for (var day = start; day < end; day = day.AddDays(1))
        {
            days.Add(new CalendarDay(day, byDay.TryGetValue(day, out var dayTodos) ? dayTodos : []));
        }

        return days;
    }
}
