using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.Trash;

public class GetTrashedTodosHandler(Database db)
{
    public async Task<IReadOnlyList<TrashedTodo>> HandleAsync()
    {
        using var conn = db.CreateConnection();
        var rows = await conn.QueryAsync<(int TrashId, string Title, int IsCompleted, int Priority, string? DueDate, int ListId, string DeletedAt)>(
            "SELECT TrashId, Title, IsCompleted, Priority, DueDate, ListId, DeletedAt FROM DeletedTodos ORDER BY DeletedAt DESC");

        return rows
            .Select(r => new TrashedTodo(
                r.TrashId,
                r.Title,
                r.IsCompleted == 1,
                (TodoPriority)r.Priority,
                r.DueDate is not null ? DateTime.Parse(r.DueDate) : (DateTime?)null,
                r.ListId,
                DateTime.Parse(r.DeletedAt)))
            .ToList();
    }
}
