using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.Subtasks;

public class DeleteSubtaskHandler(Database db)
{
    public async Task HandleAsync(int id)
    {
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM Subtasks WHERE Id = @Id", new { Id = id });
    }
}
