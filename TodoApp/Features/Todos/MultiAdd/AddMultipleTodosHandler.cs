using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.MultiAdd;

public class AddMultipleTodosHandler(Database db)
{
    public async Task<int> HandleAsync(IReadOnlyList<string> titles, int listId = 1)
    {
        var valid = titles
            .Select(t => t.Trim())
            .Where(t => t.Length > 0)
            .ToList();

        if (valid.Count == 0) return 0;

        using var conn = db.CreateConnection();
        var now = DateTime.UtcNow.ToString("O");
        foreach (var title in valid)
        {
            await conn.ExecuteAsync("""
                INSERT INTO Todos (Title, IsCompleted, CreatedAt, Priority, ListId, SortOrder)
                VALUES (@Title, 0, @CreatedAt, 0, @ListId,
                    (SELECT COALESCE(MAX(SortOrder), 0) + 1 FROM Todos))
                """,
                new { Title = title, CreatedAt = now, ListId = listId });
        }

        return valid.Count;
    }
}
