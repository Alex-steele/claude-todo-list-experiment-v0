using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.Subtasks;

public class GetSubtasksHandler(Database db)
{
    public async Task<Dictionary<int, List<Subtask>>> HandleAsync(IEnumerable<int> todoIds)
    {
        var ids = todoIds.ToList();
        if (ids.Count == 0) return [];

        using var conn = db.CreateConnection();
        var rows = await conn.QueryAsync<(int Id, int TodoId, string Title, int IsCompleted, string CreatedAt)>(
            "SELECT Id, TodoId, Title, IsCompleted, CreatedAt FROM Subtasks WHERE TodoId IN @Ids ORDER BY Id",
            new { Ids = ids });

        return rows
            .Select(r => new Subtask(r.Id, r.TodoId, r.Title, r.IsCompleted == 1, DateTime.Parse(r.CreatedAt)))
            .GroupBy(s => s.TodoId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}
