using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.UpdateNotes;

public class UpdateNotesHandler(Database db)
{
    public async Task HandleAsync(int id, string? notes)
    {
        var normalized = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

        using var conn = db.CreateConnection();
        var affected = await conn.ExecuteAsync(
            "UPDATE Todos SET Notes = @Notes WHERE Id = @Id",
            new { Id = id, Notes = normalized });

        if (affected == 0)
            throw new ArgumentException($"Todo {id} not found.", nameof(id));
    }
}
