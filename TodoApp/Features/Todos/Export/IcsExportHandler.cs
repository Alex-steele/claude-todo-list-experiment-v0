using System.Text;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.Tags;

namespace TodoApp.Features.Todos.Export;

public class IcsExportHandler
{
    public string Generate(
        IReadOnlyList<TodoSummary> todos,
        Dictionary<int, List<Tag>>? tags = null,
        DateTime? nowUtc = null)
    {
        var stamp = (nowUtc ?? DateTime.UtcNow).ToString("yyyyMMddTHHmmssZ");
        var sb = new StringBuilder();
        sb.Append("BEGIN:VCALENDAR\r\n");
        sb.Append("VERSION:2.0\r\n");
        sb.Append("PRODID:-//TodoApp//Export//EN\r\n");
        sb.Append("CALSCALE:GREGORIAN\r\n");

        foreach (var todo in todos.Where(t => t.DueDate.HasValue))
        {
            sb.Append("BEGIN:VEVENT\r\n");
            sb.Append($"UID:todo-{todo.Id}@todoapp.local\r\n");
            sb.Append($"DTSTAMP:{stamp}\r\n");
            sb.Append($"DTSTART;VALUE=DATE:{todo.DueDate!.Value:yyyyMMdd}\r\n");
            sb.Append($"SUMMARY:{Escape(todo.Title)}\r\n");
            sb.Append($"STATUS:{(todo.IsCompleted ? "COMPLETED" : "CONFIRMED")}\r\n");

            if (!string.IsNullOrWhiteSpace(todo.Notes))
                sb.Append($"DESCRIPTION:{Escape(todo.Notes)}\r\n");

            var tagNames = tags?.GetValueOrDefault(todo.Id)?.Select(t => t.Name).ToList();
            if (tagNames is { Count: > 0 })
                sb.Append($"CATEGORIES:{string.Join(",", tagNames.Select(Escape))}\r\n");

            sb.Append("END:VEVENT\r\n");
        }

        sb.Append("END:VCALENDAR\r\n");
        return sb.ToString();
    }

    private static string Escape(string value) =>
        value
            .Replace("\\", "\\\\")
            .Replace(";", "\\;")
            .Replace(",", "\\,")
            .Replace("\r\n", "\\n")
            .Replace("\n", "\\n");
}
