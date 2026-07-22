using TodoApp.Features.Todos.DueSummary;

namespace TodoApp.Features.Todos.Reminders;

public class ReminderMessageHandler
{
    public string? Handle(DueSummaryResult summary)
    {
        if (summary.Overdue == 0 && summary.DueToday == 0)
            return null;

        var parts = new List<string>();
        if (summary.Overdue > 0)
            parts.Add($"{summary.Overdue} overdue");
        if (summary.DueToday > 0)
            parts.Add($"{summary.DueToday} due today");

        return $"You have {string.Join(" and ", parts)}.";
    }
}
