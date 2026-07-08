using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using TodoApp.Features.Todos.ColorLabel;
using TodoApp.Features.Todos.TimeEstimates;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.Import;

public class JsonImportHandler(Database db)
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<int> HandleAsync(string json, int listId = 1)
    {
        var envelope = JsonSerializer.Deserialize<ImportEnvelope>(json, Options)
            ?? throw new JsonException("Root JSON object is null");

        if (envelope.Todos is null || envelope.Todos.Count == 0)
            return 0;

        int imported = 0;
        using var conn = db.CreateConnection();

        foreach (var item in envelope.Todos)
        {
            if (string.IsNullOrWhiteSpace(item.Title)) continue;

            try
            {
                var priority = ParsePriority(item.Priority);
                var colorLabel = ParseColorLabel(item.ColorLabel);
                var timeEstimate = ParseTimeEstimate(item.TimeEstimate);
                DateTime? dueDate = item.DueDate is not null ? DateTime.Parse(item.DueDate) : null;
                DateTime? completedAt = item.CompletedAt is not null ? DateTime.Parse(item.CompletedAt) : null;

                var todoId = await conn.ExecuteScalarAsync<int>(
                    """
                    INSERT INTO Todos (Title, IsCompleted, CreatedAt, Priority, DueDate, CompletedAt, IsPinned, IsBlocked, TimeEstimate, ColorLabel, Notes, ListId)
                    VALUES (@Title, @IsCompleted, @CreatedAt, @Priority, @DueDate, @CompletedAt, @IsPinned, @IsBlocked, @TimeEstimate, @ColorLabel, @Notes, @ListId);
                    SELECT last_insert_rowid();
                    """,
                    new
                    {
                        Title = item.Title,
                        IsCompleted = item.IsCompleted ? 1 : 0,
                        CreatedAt = DateTime.UtcNow.ToString("O"),
                        Priority = priority,
                        DueDate = dueDate?.ToString("O"),
                        CompletedAt = completedAt?.ToString("O"),
                        IsPinned = item.IsPinned ? 1 : 0,
                        IsBlocked = item.IsBlocked ? 1 : 0,
                        TimeEstimate = (int)timeEstimate,
                        ColorLabel = (int)colorLabel,
                        Notes = item.Notes,
                        ListId = listId
                    });

                if (item.Tags is not null)
                    foreach (var tag in item.Tags.Where(t => !string.IsNullOrWhiteSpace(t)))
                        await conn.ExecuteAsync(
                            "INSERT OR IGNORE INTO TodoTags (TodoId, Name) VALUES (@TodoId, @Name)",
                            new { TodoId = todoId, Name = tag });

                imported++;
            }
            catch
            {
                // Skip items that fail individually
            }
        }

        return imported;
    }

    private static int ParsePriority(string? value) => value?.ToLowerInvariant() switch
    {
        "high"   => 3,
        "medium" => 2,
        "low"    => 1,
        _        => 0
    };

    private static TodoColorLabel ParseColorLabel(string? value) =>
        Enum.TryParse<TodoColorLabel>(value, ignoreCase: true, out var label) ? label : TodoColorLabel.None;

    private static TimeEstimate ParseTimeEstimate(string? value) =>
        Enum.TryParse<TimeEstimate>(value, ignoreCase: true, out var est) ? est : TimeEstimate.None;
}

internal record ImportEnvelope(
    [property: JsonPropertyName("Todos")] List<ImportTodoItem>? Todos
);

internal record ImportTodoItem(
    string Title,
    bool IsCompleted,
    string? Priority,
    string? DueDate,
    string? CompletedAt,
    bool IsPinned,
    bool IsBlocked,
    string? TimeEstimate,
    string? ColorLabel,
    List<string>? Tags,
    string? Notes
);
