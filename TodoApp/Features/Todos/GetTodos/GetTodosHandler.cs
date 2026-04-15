using Dapper;
using TodoApp.Features.Todos.RecurringTodos;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.GetTodos;

public record TodoSummary(int Id, string Title, bool IsCompleted, DateTime CreatedAt, TodoPriority Priority, DateTime? DueDate, bool IsPinned = false, string? Notes = null, RecurrenceRule Recurrence = RecurrenceRule.None, int ListId = 1);

public class GetTodosHandler(Database db)
{
    public async Task<IReadOnlyList<TodoSummary>> HandleAsync()
    {
        using var conn = db.CreateConnection();
        var rows = await conn.QueryAsync<(int Id, string Title, int IsCompleted, string CreatedAt, int Priority, string? DueDate, int IsPinned, string? Notes, int Recurrence, int ListId)>(
            "SELECT Id, Title, IsCompleted, CreatedAt, Priority, DueDate, IsPinned, Notes, Recurrence, ListId FROM Todos ORDER BY Id DESC");

        return rows
            .Select(r => new TodoSummary(
                r.Id,
                r.Title,
                r.IsCompleted == 1,
                DateTime.Parse(r.CreatedAt),
                (TodoPriority)r.Priority,
                r.DueDate is not null ? DateTime.Parse(r.DueDate) : (DateTime?)null,
                r.IsPinned == 1,
                r.Notes,
                (RecurrenceRule)r.Recurrence,
                r.ListId))
            .ToList();
    }
}
