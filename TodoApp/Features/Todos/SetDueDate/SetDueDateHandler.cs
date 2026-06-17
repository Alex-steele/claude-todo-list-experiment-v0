using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.SetDueDate;

public class SetDueDateHandler(Database db)
{
    public async Task HandleAsync(int id, DateTime? dueDate)
    {
        using var connection = db.CreateConnection();
        var affected = await connection.ExecuteAsync(
            "UPDATE Todos SET DueDate = @dueDate WHERE Id = @id",
            new { dueDate = dueDate.HasValue ? dueDate.Value.Date.ToString("o") : (string?)null, id });

        if (affected == 0)
            throw new ArgumentException($"Todo with id {id} not found.", nameof(id));
    }
}
