using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.GetTodos;

public record TodoSummary(int Id, string Title, bool IsCompleted, DateTime CreatedAt);

public class GetTodosHandler(Database db)
{
    public async Task<IReadOnlyList<TodoSummary>> HandleAsync()
    {
        using var conn = db.CreateConnection();
        var rows = await conn.QueryAsync<(int Id, string Title, int IsCompleted, string CreatedAt)>(
            "SELECT Id, Title, IsCompleted, CreatedAt FROM Todos ORDER BY Id DESC");

        return rows
            .Select(r => new TodoSummary(r.Id, r.Title, r.IsCompleted == 1, DateTime.Parse(r.CreatedAt)))
            .ToList();
    }
}
