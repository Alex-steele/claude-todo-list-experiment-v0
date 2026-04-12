using Dapper;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.RecurringTodos;

public class CreateRecurringInstanceHandler(Database db)
{
    public async Task<int> HandleAsync(TodoSummary completed)
    {
        if (completed.Recurrence == RecurrenceRule.None)
            throw new ArgumentException("Todo is not recurring.");

        var nextDueDate = ComputeNextDueDate(completed.DueDate, completed.Recurrence);

        using var conn = db.CreateConnection();
        var id = await conn.ExecuteScalarAsync<int>("""
            INSERT INTO Todos (Title, CreatedAt, Priority, DueDate, Recurrence, Notes)
            VALUES (@Title, @CreatedAt, @Priority, @DueDate, @Recurrence, @Notes);
            SELECT last_insert_rowid();
            """, new
        {
            completed.Title,
            CreatedAt = DateTime.UtcNow.ToString("O"),
            Priority = (int)completed.Priority,
            DueDate = nextDueDate.HasValue ? nextDueDate.Value.ToString("O") : (string?)null,
            Recurrence = (int)completed.Recurrence,
            completed.Notes
        });

        return id;
    }

    public static DateTime? ComputeNextDueDate(DateTime? currentDueDate, RecurrenceRule rule)
    {
        var baseDate = currentDueDate?.Date ?? DateTime.Today;

        // If overdue, advance from today instead of the past due date
        if (baseDate < DateTime.Today)
            baseDate = DateTime.Today;

        return rule switch
        {
            RecurrenceRule.Daily   => baseDate.AddDays(1),
            RecurrenceRule.Weekly  => baseDate.AddDays(7),
            RecurrenceRule.Monthly => baseDate.AddMonths(1),
            _                      => (DateTime?)null
        };
    }
}
