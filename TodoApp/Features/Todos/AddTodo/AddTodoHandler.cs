using Dapper;
using TodoApp.Features.Todos.RecurringTodos;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.AddTodo;

public class AddTodoHandler(Database db)
{
    public async Task<int> HandleAsync(string title, TodoPriority priority = TodoPriority.None, DateTime? dueDate = null, RecurrenceRule recurrence = RecurrenceRule.None, int listId = 1)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.");

        using var conn = db.CreateConnection();
        var id = await conn.ExecuteScalarAsync<int>("""
            INSERT INTO Todos (Title, CreatedAt, Priority, DueDate, Recurrence, ListId)
            VALUES (@Title, @CreatedAt, @Priority, @DueDate, @Recurrence, @ListId);
            SELECT last_insert_rowid();
            """, new
            {
                Title = title.Trim(),
                CreatedAt = DateTime.UtcNow.ToString("O"),
                Priority = (int)priority,
                DueDate = dueDate.HasValue ? dueDate.Value.ToString("O") : (string?)null,
                Recurrence = (int)recurrence,
                ListId = listId
            });

        return id;
    }
}
