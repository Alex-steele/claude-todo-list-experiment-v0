using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.SnoozeTodo;

public class SnoozeTodoHandler(Database db)
{
    public async Task HandleAsync(int id, DateTime newDueDate)
    {
        using var connection = db.CreateConnection();
        var affected = await connection.ExecuteAsync(
            "UPDATE Todos SET DueDate = @dueDate WHERE Id = @id",
            new { dueDate = newDueDate.Date.ToString("o"), id });

        if (affected == 0)
            throw new ArgumentException($"Todo with id {id} not found.", nameof(id));
    }
}
