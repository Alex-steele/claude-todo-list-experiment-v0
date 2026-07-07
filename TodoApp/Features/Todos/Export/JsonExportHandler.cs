using System.Text.Json;
using System.Text.Json.Serialization;
using TodoApp.Features.Todos.ColorLabel;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.Tags;
using TodoApp.Features.Todos.TimeEstimates;

namespace TodoApp.Features.Todos.Export;

public class JsonExportHandler
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string Generate(
        string listName,
        IReadOnlyList<TodoSummary> todos,
        Dictionary<int, List<Tag>>? tags = null)
    {
        var items = todos.Select(t => new TodoJson(
            t.Id,
            t.Title,
            t.IsCompleted,
            t.Priority.ToString().ToLowerInvariant(),
            t.DueDate?.ToString("yyyy-MM-dd"),
            t.CompletedAt?.ToString("O"),
            t.CreatedAt.ToString("O"),
            t.IsPinned,
            t.IsBlocked,
            t.TimeEstimate == TimeEstimate.None ? null : t.TimeEstimate.ToString(),
            t.ColorLabel == TodoColorLabel.None ? null : t.ColorLabel.ToString().ToLowerInvariant(),
            tags?.GetValueOrDefault(t.Id)?.Select(tag => tag.Name).ToList() ?? [],
            t.Notes
        )).ToList();

        var envelope = new ExportEnvelope(
            listName,
            DateTime.Today.ToString("yyyy-MM-dd"),
            new CountSummary(
                todos.Count,
                todos.Count(t => !t.IsCompleted),
                todos.Count(t => t.IsCompleted)),
            items
        );

        return JsonSerializer.Serialize(envelope, Options);
    }
}

internal record CountSummary(int Total, int Active, int Completed);

internal record ExportEnvelope(
    string List,
    string Exported,
    CountSummary Count,
    List<TodoJson> Todos
);

internal record TodoJson(
    int Id,
    string Title,
    bool IsCompleted,
    string Priority,
    string? DueDate,
    string? CompletedAt,
    string CreatedAt,
    bool IsPinned,
    bool IsBlocked,
    string? TimeEstimate,
    string? ColorLabel,
    List<string> Tags,
    string? Notes
);
