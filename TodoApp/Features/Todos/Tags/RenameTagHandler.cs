using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.Tags;

public class RenameTagHandler(Database db)
{
    public async Task HandleAsync(string oldName, string newName)
    {
        oldName = oldName.Trim().ToLowerInvariant();
        newName = newName.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Tag name cannot be empty.", nameof(newName));
        if (oldName == newName) return;

        using var conn = db.CreateConnection();

        // Remove old-name rows on todos that already have the new name (prevent duplicates)
        await conn.ExecuteAsync("""
            DELETE FROM TodoTags
            WHERE Name = @OldName
              AND EXISTS (
                  SELECT 1 FROM TodoTags t2
                  WHERE t2.TodoId = TodoTags.TodoId AND t2.Name = @NewName
              )
            """, new { OldName = oldName, NewName = newName });

        await conn.ExecuteAsync(
            "UPDATE TodoTags SET Name = @NewName WHERE Name = @OldName",
            new { OldName = oldName, NewName = newName });
    }
}
