using Dapper;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.UndoRedo;

public class RestoreTodosHandler(Database db)
{
    public async Task HandleAsync(IReadOnlyList<TodoSummary> todos)
    {
        if (todos.Count == 0) return;

        using var conn = db.CreateConnection();
        foreach (var todo in todos)
        {
            await conn.ExecuteAsync("""
                INSERT INTO Todos (Id, Title, IsCompleted, CreatedAt, Priority, DueDate, IsPinned, Notes, Recurrence, ListId)
                VALUES (@Id, @Title, @IsCompleted, @CreatedAt, @Priority, @DueDate, @IsPinned, @Notes, @Recurrence, @ListId)
                ON CONFLICT(Id) DO NOTHING
                """, new
            {
                todo.Id,
                todo.Title,
                IsCompleted = todo.IsCompleted ? 1 : 0,
                CreatedAt = todo.CreatedAt.ToString("O"),
                Priority = (int)todo.Priority,
                DueDate = todo.DueDate.HasValue ? todo.DueDate.Value.ToString("O") : (string?)null,
                IsPinned = todo.IsPinned ? 1 : 0,
                todo.Notes,
                Recurrence = (int)todo.Recurrence,
                todo.ListId
            });
        }
    }
}
