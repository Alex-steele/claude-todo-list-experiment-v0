using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Lists;

public class GetListsHandler(Database db)
{
    public async Task<IReadOnlyList<TodoList>> HandleAsync()
    {
        using var conn = db.CreateConnection();
        var rows = await conn.QueryAsync<(int Id, string Name)>(
            "SELECT Id, Name FROM TodoLists ORDER BY Id");
        return rows.Select(r => new TodoList(r.Id, r.Name)).ToList();
    }
}
