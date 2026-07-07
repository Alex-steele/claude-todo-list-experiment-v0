using System.Text.RegularExpressions;
using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.Import;

public class MarkdownImportHandler(Database db)
{
    private static readonly Regex TodoLinePattern =
        new(@"^- \[(?<done>[x ])\] (?<rest>.+)$", RegexOptions.Compiled);

    private static readonly Regex MetaPattern =
        new(@"\s*_\((?<meta>[^)]+)\)_\s*$", RegexOptions.Compiled);

    private static readonly Regex StrikethroughPattern =
        new(@"^~~(?<inner>.+)~~$", RegexOptions.Compiled);

    /// <summary>
    /// Parses a markdown string (in the format produced by MarkdownExportHandler) and inserts the todos.
    /// Returns the number of successfully imported todos.
    /// </summary>
    public async Task<int> HandleAsync(string markdownContent, int listId = 1)
    {
        var lines = markdownContent.ReplaceLineEndings("\n")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);

        int imported = 0;
        using var conn = db.CreateConnection();

        foreach (var line in lines)
        {
            var todoMatch = TodoLinePattern.Match(line.Trim());
            if (!todoMatch.Success) continue;

            var isCompleted = todoMatch.Groups["done"].Value == "x";
            var rest = todoMatch.Groups["rest"].Value;

            // Extract and strip trailing metadata block: _(...)_
            string? metaStr = null;
            var metaMatch = MetaPattern.Match(rest);
            if (metaMatch.Success)
            {
                metaStr = metaMatch.Groups["meta"].Value;
                rest = rest[..metaMatch.Index].Trim();
            }

            // Strip ~~strikethrough~~ wrapping used for completed todos
            var strikeMatch = StrikethroughPattern.Match(rest);
            if (strikeMatch.Success)
                rest = strikeMatch.Groups["inner"].Value;

            var title = rest.Trim();
            if (string.IsNullOrWhiteSpace(title)) continue;

            var priority = 0;
            DateTime? dueDate = null;
            var tags = new List<string>();

            if (metaStr is not null)
            {
                foreach (var part in metaStr.Split('·').Select(p => p.Trim()))
                {
                    if (part.StartsWith('#'))
                    {
                        var tagName = part[1..].Trim();
                        if (tagName.Length > 0) tags.Add(tagName);
                    }
                    else if (part.Equals("high", StringComparison.OrdinalIgnoreCase))
                        priority = 3;
                    else if (part.Equals("medium", StringComparison.OrdinalIgnoreCase))
                        priority = 2;
                    else if (part.Equals("low", StringComparison.OrdinalIgnoreCase))
                        priority = 1;
                    else if (part.Equals("due today", StringComparison.OrdinalIgnoreCase))
                        dueDate = DateTime.Today;
                    else if (part.StartsWith("due ", StringComparison.OrdinalIgnoreCase) &&
                             DateTime.TryParse(part[4..].Trim(), out var dueD))
                        dueDate = dueD;
                    else if (part.StartsWith("overdue ", StringComparison.OrdinalIgnoreCase) &&
                             DateTime.TryParse(part[8..].Trim(), out var overdueD))
                        dueDate = overdueD;
                }
            }

            try
            {
                var todoId = await conn.ExecuteScalarAsync<int>(
                    """
                    INSERT INTO Todos (Title, IsCompleted, CreatedAt, Priority, DueDate, ListId)
                    VALUES (@Title, @IsCompleted, @CreatedAt, @Priority, @DueDate, @ListId);
                    SELECT last_insert_rowid();
                    """,
                    new
                    {
                        Title = title,
                        IsCompleted = isCompleted ? 1 : 0,
                        CreatedAt = DateTime.UtcNow.ToString("O"),
                        Priority = priority,
                        DueDate = dueDate?.ToString("O"),
                        ListId = listId
                    });

                foreach (var tag in tags)
                    await conn.ExecuteAsync(
                        "INSERT OR IGNORE INTO TodoTags (TodoId, Name) VALUES (@TodoId, @Name)",
                        new { TodoId = todoId, Name = tag });

                imported++;
            }
            catch
            {
                // Skip rows that fail to insert
            }
        }

        return imported;
    }
}
