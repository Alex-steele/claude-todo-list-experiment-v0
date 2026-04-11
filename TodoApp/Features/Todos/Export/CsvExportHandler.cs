using System.Text;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.Tags;

namespace TodoApp.Features.Todos.Export;

public class CsvExportHandler
{
    public string Generate(
        IReadOnlyList<TodoSummary> todos,
        Dictionary<int, List<Tag>>? tags = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,Title,Priority,DueDate,IsCompleted,CreatedAt,Tags,Notes");

        foreach (var todo in todos)
        {
            var tagNames = tags?.GetValueOrDefault(todo.Id)?.Select(t => t.Name) ?? [];
            var tagsCell = string.Join("|", tagNames);

            sb.AppendLine(string.Join(",", [
                todo.Id.ToString(),
                EscapeCsv(todo.Title),
                todo.Priority.ToString(),
                todo.DueDate?.ToString("yyyy-MM-dd") ?? "",
                todo.IsCompleted.ToString(),
                todo.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                EscapeCsv(tagsCell),
                EscapeCsv(todo.Notes ?? "")
            ]));
        }

        return sb.ToString();
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
