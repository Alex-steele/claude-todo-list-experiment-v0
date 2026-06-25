using TodoApp.Features.Todos.RecurringTodos;
using TodoApp.Features.Todos.TimeEstimates;

namespace TodoApp.Features.Todos.Templates;

public record TodoTemplate(
    int Id,
    string Name,
    TodoPriority Priority,
    TimeEstimate TimeEstimate,
    RecurrenceRule Recurrence);
