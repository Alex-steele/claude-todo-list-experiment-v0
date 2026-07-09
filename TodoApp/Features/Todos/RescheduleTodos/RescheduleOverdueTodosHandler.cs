using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.RescheduleTodos;

public class RescheduleOverdueTodosHandler(Database db)
{
    /// <summary>
    /// Moves all overdue, non-completed todos in <paramref name="listId"/> to <paramref name="targetDate"/>.
    /// Returns the number of todos rescheduled.
    /// </summary>
    public async Task<int> HandleAsync(DateTime targetDate, int listId)
    {
        var today = DateTime.UtcNow.Date;
        var target = targetDate.Date.ToString("O");

        using var conn = db.CreateConnection();
        return await conn.ExecuteAsync(
            """
            UPDATE Todos
            SET    DueDate = @Target
            WHERE  IsCompleted = 0
              AND  ListId     = @ListId
              AND  DueDate IS NOT NULL
              AND  DATE(DueDate) < @Today
            """,
            new { Target = target, ListId = listId, Today = today.ToString("yyyy-MM-dd") });
    }
}
