using Dapper;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.ClearCompleted;

public class ClearCompletedHandler(Database db)
{
    /// <summary>
    /// Deletes all completed todos and returns them as a snapshot for undo.
    /// </summary>
    public async Task<IReadOnlyList<TodoSummary>> HandleAsync()
    {
        using var conn = db.CreateConnection();

        var rows = await conn.QueryAsync<(int Id, string Title, int IsCompleted, string CreatedAt, int Priority, string? DueDate, int IsPinned, string? Notes)>(
            "SELECT Id, Title, IsCompleted, CreatedAt, Priority, DueDate, IsPinned, Notes FROM Todos WHERE IsCompleted = 1");

        var completed = rows
            .Select(r => new TodoSummary(
                r.Id,
                r.Title,
                r.IsCompleted == 1,
                DateTime.Parse(r.CreatedAt),
                (TodoPriority)r.Priority,
                r.DueDate is not null ? DateTime.Parse(r.DueDate) : (DateTime?)null,
                r.IsPinned == 1,
                r.Notes))
            .ToList();

        if (completed.Count == 0) return [];

        await conn.ExecuteAsync("DELETE FROM Todos WHERE IsCompleted = 1");
        return completed;
    }
}
