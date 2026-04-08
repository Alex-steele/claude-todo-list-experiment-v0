using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.Tags;

public class GetTodoTagsHandler(Database db)
{
    public async Task<Dictionary<int, List<Tag>>> HandleAsync(IEnumerable<int> todoIds)
    {
        var ids = todoIds.ToList();
        if (ids.Count == 0) return [];

        using var conn = db.CreateConnection();
        var rows = await conn.QueryAsync<(int Id, int TodoId, string Name)>(
            "SELECT Id, TodoId, Name FROM TodoTags WHERE TodoId IN @Ids ORDER BY Name",
            new { Ids = ids });

        return rows
            .Select(r => new Tag(r.Id, r.TodoId, r.Name))
            .GroupBy(t => t.TodoId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}
