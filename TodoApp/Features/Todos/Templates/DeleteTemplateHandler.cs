using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.Templates;

public class DeleteTemplateHandler(Database db)
{
    public async Task HandleAsync(int id)
    {
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM TodoTemplates WHERE Id = @Id", new { Id = id });
    }
}
