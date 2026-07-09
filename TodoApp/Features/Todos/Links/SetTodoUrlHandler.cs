using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.Links;

public class SetTodoUrlHandler(Database db)
{
    public async Task HandleAsync(int id, string? url)
    {
        if (url is not null)
        {
            url = url.Trim();
            if (url.Length == 0) url = null;
        }

        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Todos SET Url = @Url WHERE Id = @Id",
            new { Url = url, Id = id });
    }
}
