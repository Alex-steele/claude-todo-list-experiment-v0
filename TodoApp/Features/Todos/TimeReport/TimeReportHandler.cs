using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.TimeTracking;

namespace TodoApp.Features.Todos.TimeReport;

public record TimeReportEntry(int TodoId, string Title, int Seconds, bool IsCompleted, bool IsRunning);

public record TimeReportResult(int TotalSeconds, IReadOnlyList<TimeReportEntry> Entries)
{
    public bool HasData => TotalSeconds > 0;
}

public class TimeReportHandler
{
    public TimeReportResult Handle(IReadOnlyList<TodoSummary> todos, int listId, DateTime now)
    {
        var entries = todos
            .Where(t => t.ListId == listId)
            .Select(t => new TimeReportEntry(
                t.Id,
                t.Title,
                TimeTrackingCalculator.GetElapsedSeconds(t, now),
                t.IsCompleted,
                t.TimerStartedAt is not null))
            .Where(e => e.Seconds > 0)
            .OrderByDescending(e => e.Seconds)
            .ToList();

        return new TimeReportResult(entries.Sum(e => e.Seconds), entries);
    }
}
