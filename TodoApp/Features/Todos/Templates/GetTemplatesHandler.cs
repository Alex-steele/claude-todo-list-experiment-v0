using Dapper;
using TodoApp.Features.Todos.RecurringTodos;
using TodoApp.Features.Todos.TimeEstimates;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.Templates;

public class GetTemplatesHandler(Database db)
{
    public async Task<IReadOnlyList<TodoTemplate>> HandleAsync()
    {
        using var conn = db.CreateConnection();
        var rows = await conn.QueryAsync(
            "SELECT Id, Name, Priority, TimeEstimate, Recurrence FROM TodoTemplates ORDER BY Id");
        return rows.Select(r => new TodoTemplate(
            (int)r.Id,
            (string)r.Name,
            (TodoPriority)(int)r.Priority,
            (TimeEstimate)(int)r.TimeEstimate,
            (RecurrenceRule)(int)r.Recurrence)).ToList();
    }
}
