using System.Text;
using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.Import;

public class ImportTodosHandler(Database db)
{
    /// <summary>
    /// Parses a CSV string (in the format produced by CsvExportHandler) and inserts the todos.
    /// Returns the number of successfully imported todos.
    /// </summary>
    public async Task<int> HandleAsync(string csvContent)
    {
        var lines = csvContent.ReplaceLineEndings("\n")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length <= 1) return 0;

        // Validate header
        if (!lines[0].TrimStart().StartsWith("Id,Title", StringComparison.OrdinalIgnoreCase))
            return 0;

        int imported = 0;
        using var conn = db.CreateConnection();

        foreach (var line in lines.Skip(1))
        {
            var fields = ParseCsvRow(line);
            if (fields.Length < 2) continue;

            var title = fields[1].Trim();
            if (string.IsNullOrWhiteSpace(title)) continue;

            try
            {
                var priority = fields.Length > 2 ? ParsePriority(fields[2]) : 0;
                DateTime? dueDate = fields.Length > 3 && !string.IsNullOrWhiteSpace(fields[3])
                    && DateTime.TryParse(fields[3], out var d) ? d : null;
                var isCompleted = fields.Length > 4
                    && fields[4].Trim().Equals("True", StringComparison.OrdinalIgnoreCase);
                var tagNames = fields.Length > 6 && !string.IsNullOrWhiteSpace(fields[6])
                    ? fields[6].Split('|', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim().ToLowerInvariant())
                        .Where(t => t.Length > 0)
                        .ToList()
                    : [];
                var notes = fields.Length > 7 && !string.IsNullOrWhiteSpace(fields[7])
                    ? fields[7].Trim() : null;

                var todoId = await conn.ExecuteScalarAsync<int>(
                    """
                    INSERT INTO Todos (Title, IsCompleted, CreatedAt, Priority, DueDate, Notes)
                    VALUES (@Title, @IsCompleted, @CreatedAt, @Priority, @DueDate, @Notes);
                    SELECT last_insert_rowid();
                    """,
                    new
                    {
                        Title = title,
                        IsCompleted = isCompleted ? 1 : 0,
                        CreatedAt = DateTime.UtcNow.ToString("O"),
                        Priority = priority,
                        DueDate = dueDate?.ToString("O"),
                        Notes = notes
                    });

                foreach (var tag in tagNames)
                    await conn.ExecuteAsync(
                        "INSERT OR IGNORE INTO TodoTags (TodoId, Name) VALUES (@TodoId, @Name)",
                        new { TodoId = todoId, Name = tag });

                imported++;
            }
            catch
            {
                // Skip rows that fail to parse or insert
            }
        }

        return imported;
    }

    private static int ParsePriority(string value) => value.Trim() switch
    {
        "Low"    => 1,
        "Medium" => 2,
        "High"   => 3,
        _        => 0
    };

    /// <summary>
    /// Parses a single CSV row, handling double-quote escaping per RFC 4180.
    /// </summary>
    private static string[] ParseCsvRow(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        fields.Add(current.ToString());
        return [.. fields];
    }
}
