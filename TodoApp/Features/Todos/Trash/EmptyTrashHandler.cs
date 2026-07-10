using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.Trash;

public class EmptyTrashHandler(Database db)
{
    public async Task<int> HandleAsync()
    {
        using var conn = db.CreateConnection();
        return await conn.ExecuteAsync("DELETE FROM DeletedTodos");
    }
}
