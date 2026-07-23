using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Lists;

public class GetListsHandler(Database db)
{
    public async Task<IReadOnlyList<TodoList>> HandleAsync()
    {
        using var conn = db.CreateConnection();
        var rows = await conn.QueryAsync<(int Id, string Name, int Color)>(
            "SELECT Id, Name, Color FROM TodoLists WHERE IsArchived = 0 ORDER BY SortOrder ASC, Id ASC");
        return rows.Select(r => new TodoList(r.Id, r.Name, Color: (ListColor)r.Color)).ToList();
    }
}
