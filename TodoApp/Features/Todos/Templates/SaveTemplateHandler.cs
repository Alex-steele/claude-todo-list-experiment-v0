using Dapper;
using TodoApp.Features.Todos.RecurringTodos;
using TodoApp.Features.Todos.TimeEstimates;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.Templates;

public class SaveTemplateHandler(Database db)
{
    public async Task<int> HandleAsync(
        string name,
        TodoPriority priority,
        TimeEstimate timeEstimate,
        RecurrenceRule recurrence)
    {
        var trimmed = name.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new ArgumentException("Template name cannot be empty.", nameof(name));

        using var conn = db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            INSERT INTO TodoTemplates (Name, Priority, TimeEstimate, Recurrence)
            VALUES (@Name, @Priority, @TimeEstimate, @Recurrence);
            SELECT last_insert_rowid();
            """,
            new { Name = trimmed, Priority = (int)priority, TimeEstimate = (int)timeEstimate, Recurrence = (int)recurrence });
    }
}
