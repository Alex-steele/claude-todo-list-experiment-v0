using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.Tags;

public class AddTagHandler(Database db)
{
    public async Task HandleAsync(int todoId, string name)
    {
        name = name.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tag name cannot be empty.", nameof(name));

        using var conn = db.CreateConnection();
        var exists = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM TodoTags WHERE TodoId = @TodoId AND Name = @Name",
            new { TodoId = todoId, Name = name });

        if (exists == 0)
            await conn.ExecuteAsync(
                "INSERT INTO TodoTags (TodoId, Name) VALUES (@TodoId, @Name)",
                new { TodoId = todoId, Name = name });
    }
}
