using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.EditTodo;

public class EditTodoHandler(Database db)
{
    public async Task HandleAsync(int id, string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new ArgumentException("Title cannot be empty.", nameof(newTitle));

        using var connection = db.CreateConnection();
        var affected = await connection.ExecuteAsync(
            "UPDATE Todos SET Title = @title WHERE Id = @id",
            new { title = newTitle.Trim(), id });

        if (affected == 0)
            throw new ArgumentException($"Todo with id {id} not found.", nameof(id));
    }
}
