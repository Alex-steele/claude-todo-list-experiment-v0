using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.RecurringTodos;

public class SkipRecurrenceHandler(Database db)
{
    public async Task<DateTime> HandleAsync(int todoId, DateTime? currentDueDate, RecurrenceRule recurrence)
    {
        if (recurrence == RecurrenceRule.None)
            throw new ArgumentException("Todo is not recurring.", nameof(recurrence));

        var nextDueDate = CreateRecurringInstanceHandler.ComputeNextDueDate(currentDueDate, recurrence)!.Value;

        using var conn = db.CreateConnection();
        var affected = await conn.ExecuteAsync(
            "UPDATE Todos SET DueDate = @DueDate WHERE Id = @Id",
            new { DueDate = nextDueDate.ToString("O"), Id = todoId });

        if (affected == 0)
            throw new ArgumentException($"Todo {todoId} not found.", nameof(todoId));

        return nextDueDate;
    }
}
