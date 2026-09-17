using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.Assignee;

public class SetAssigneeHandler(Database db)
{
    public async Task HandleAsync(int id, string? assignee)
    {
        if (assignee is not null)
        {
            assignee = assignee.Trim();
            if (assignee.Length == 0) assignee = null;
        }

        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Todos SET Assignee = @Assignee WHERE Id = @Id",
            new { Assignee = assignee, Id = id });
    }
}
