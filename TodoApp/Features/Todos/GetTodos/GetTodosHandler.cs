using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.GetTodos;

public record TodoSummary(int Id, string Title, bool IsCompleted, DateTime CreatedAt, TodoPriority Priority, DateTime? DueDate);

public class GetTodosHandler(Database db)
{
    public async Task<IReadOnlyList<TodoSummary>> HandleAsync()
    {
        using var conn = db.CreateConnection();
        var rows = await conn.QueryAsync<(int Id, string Title, int IsCompleted, string CreatedAt, int Priority, string? DueDate)>(
            "SELECT Id, Title, IsCompleted, CreatedAt, Priority, DueDate FROM Todos ORDER BY Id DESC");

        return rows
            .Select(r => new TodoSummary(
                r.Id,
                r.Title,
                r.IsCompleted == 1,
                DateTime.Parse(r.CreatedAt),
                (TodoPriority)r.Priority,
                r.DueDate is not null ? DateTime.Parse(r.DueDate) : (DateTime?)null))
            .ToList();
    }
}
