using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.Tags;

public class GetAllTagNamesHandler(Database db)
{
    public async Task<IReadOnlyList<string>> HandleAsync()
    {
        using var conn = db.CreateConnection();
        var names = await conn.QueryAsync<string>(
            "SELECT DISTINCT Name FROM TodoTags ORDER BY Name");
        return names.AsList();
    }
}
