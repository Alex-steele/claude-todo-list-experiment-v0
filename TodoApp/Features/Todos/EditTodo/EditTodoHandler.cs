using Dapper;
using TodoApp.Features.Todos;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.EditTodo;

public class EditTodoHandler(Database db)
{
    public async Task HandleAsync(int id, string newTitle, TodoPriority? priority = null, DateTime? dueDate = null)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new ArgumentException("Title cannot be empty.", nameof(newTitle));

        using var connection = db.CreateConnection();
        var affected = await connection.ExecuteAsync(
            """
            UPDATE Todos
            SET Title    = @title,
                Priority = @priority,
                DueDate  = @dueDate
            WHERE Id = @id
            """,
            new
            {
                title    = newTitle.Trim(),
                priority = (int)(priority ?? TodoPriority.None),
                dueDate  = dueDate.HasValue ? dueDate.Value.ToString("o") : (string?)null,
                id
            });

        if (affected == 0)
            throw new ArgumentException($"Todo with id {id} not found.", nameof(id));
    }
}
