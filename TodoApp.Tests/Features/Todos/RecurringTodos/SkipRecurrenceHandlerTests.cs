using TodoApp.Features.Todos;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.RecurringTodos;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.RecurringTodos;

public class SkipRecurrenceHandlerTests
{
    [Fact]
    public async Task Handle_AdvancesDueDateWithoutCreatingNewTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getHandler = new GetTodosHandler(db);
        var skipHandler = new SkipRecurrenceHandler(db);

        var dueDate = DateTime.Today.AddDays(3);
        var id = await addHandler.HandleAsync("Daily standup", dueDate: dueDate, recurrence: RecurrenceRule.Daily);

        await skipHandler.HandleAsync(id, dueDate, RecurrenceRule.Daily);

        var after = await getHandler.HandleAsync();
        Assert.Single(after);
        var todo = after.Single();
        Assert.Equal(id, todo.Id);
        Assert.False(todo.IsCompleted);
        Assert.Equal(dueDate.AddDays(1).Date, todo.DueDate!.Value.Date);
    }

    [Fact]
    public async Task Handle_Weekly_AdvancesBySevenDays()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getHandler = new GetTodosHandler(db);
        var skipHandler = new SkipRecurrenceHandler(db);

        var dueDate = DateTime.Today.AddDays(2);
        var id = await addHandler.HandleAsync("Weekly review", dueDate: dueDate, recurrence: RecurrenceRule.Weekly);

        await skipHandler.HandleAsync(id, dueDate, RecurrenceRule.Weekly);

        var after = await getHandler.HandleAsync();
        var todo = after.Single(t => t.Id == id);
        Assert.Equal(dueDate.AddDays(7).Date, todo.DueDate!.Value.Date);
    }

    [Fact]
    public async Task Handle_DoesNotMarkTodoCompleted()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getHandler = new GetTodosHandler(db);
        var skipHandler = new SkipRecurrenceHandler(db);

        var id = await addHandler.HandleAsync("Daily standup", recurrence: RecurrenceRule.Daily);

        await skipHandler.HandleAsync(id, null, RecurrenceRule.Daily);

        var todo = (await getHandler.HandleAsync()).Single(t => t.Id == id);
        Assert.False(todo.IsCompleted);
        Assert.Null(todo.CompletedAt);
    }

    [Fact]
    public async Task Handle_NoneRecurrence_Throws()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var skipHandler = new SkipRecurrenceHandler(db);

        var id = await addHandler.HandleAsync("One-time task");

        await Assert.ThrowsAsync<ArgumentException>(() =>
            skipHandler.HandleAsync(id, null, RecurrenceRule.None));
    }

    [Fact]
    public async Task Handle_UnknownTodoId_Throws()
    {
        var db = await TestDatabase.CreateAsync();
        var skipHandler = new SkipRecurrenceHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            skipHandler.HandleAsync(9999, null, RecurrenceRule.Daily));
    }

    [Fact]
    public async Task Handle_Weekday_SkipsToMondayFromFriday()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getHandler = new GetTodosHandler(db);
        var skipHandler = new SkipRecurrenceHandler(db);

        var friday = NextDateForDayOfWeek(DayOfWeek.Friday);
        var id = await addHandler.HandleAsync("Standup", dueDate: friday, recurrence: RecurrenceRule.Weekday);

        await skipHandler.HandleAsync(id, friday, RecurrenceRule.Weekday);

        var todo = (await getHandler.HandleAsync()).Single(t => t.Id == id);
        Assert.Equal(DayOfWeek.Monday, todo.DueDate!.Value.DayOfWeek);
    }

    private static DateTime NextDateForDayOfWeek(DayOfWeek day)
    {
        var date = DateTime.Today.AddDays(14);
        while (date.DayOfWeek != day)
            date = date.AddDays(1);
        return date;
    }
}
