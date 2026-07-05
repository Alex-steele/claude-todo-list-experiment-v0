using System.Text;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.Tags;

namespace TodoApp.Features.Todos.Export;

public class MarkdownExportHandler
{
    public string Generate(
        string listName,
        IReadOnlyList<TodoSummary> todos,
        Dictionary<int, List<Tag>>? tags = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {listName}");
        sb.AppendLine();
        sb.AppendLine($"_Exported {DateTime.Today:yyyy-MM-dd}_");
        sb.AppendLine();

        var active = todos
            .Where(t => !t.IsCompleted)
            .OrderBy(t => t.IsPinned ? 0 : 1)
            .ThenBy(t => t.Priority switch
            {
                TodoPriority.High   => 0,
                TodoPriority.Medium => 1,
                TodoPriority.Low    => 2,
                _                  => 3
            })
            .ThenBy(t => t.DueDate ?? DateTime.MaxValue)
            .ToList();

        var completed = todos
            .Where(t => t.IsCompleted)
            .OrderByDescending(t => t.CompletedAt ?? t.CreatedAt)
            .ToList();

        if (active.Count > 0)
        {
            sb.AppendLine("## Active");
            sb.AppendLine();
            foreach (var todo in active)
                sb.AppendLine(FormatTodo(todo, isCompleted: false, tags));
            sb.AppendLine();
        }

        if (completed.Count > 0)
        {
            sb.AppendLine("## Completed");
            sb.AppendLine();
            foreach (var todo in completed)
                sb.AppendLine(FormatTodo(todo, isCompleted: true, tags));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string FormatTodo(TodoSummary todo, bool isCompleted, Dictionary<int, List<Tag>>? tags)
    {
        var check = isCompleted ? "x" : " ";
        var title = isCompleted ? $"~~{todo.Title}~~" : todo.Title;

        var meta = new List<string>();

        if (todo.IsPinned)
            meta.Add("📌");

        if (todo.Priority != TodoPriority.None)
            meta.Add(todo.Priority.ToString().ToLower());

        if (todo.DueDate.HasValue)
        {
            var today = DateTime.Today;
            var due = todo.DueDate.Value.Date;
            if (due < today)
                meta.Add($"overdue {due:yyyy-MM-dd}");
            else if (due == today)
                meta.Add("due today");
            else
                meta.Add($"due {due:yyyy-MM-dd}");
        }

        var todoTags = tags?.GetValueOrDefault(todo.Id);
        if (todoTags is { Count: > 0 })
            meta.AddRange(todoTags.Select(t => $"#{t.Name}"));

        var metaStr = meta.Count > 0 ? $" _({string.Join(" · ", meta)})_" : "";
        return $"- [{check}] {title}{metaStr}";
    }
}
