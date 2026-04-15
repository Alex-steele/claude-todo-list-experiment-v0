using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Lists;

public class CreateListHandler(Database db)
{
    public async Task<int> HandleAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("List name cannot be empty.");

        using var conn = db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>("""
            INSERT INTO TodoLists (Name) VALUES (@Name);
            SELECT last_insert_rowid();
            """, new { Name = name.Trim() });
    }
}
