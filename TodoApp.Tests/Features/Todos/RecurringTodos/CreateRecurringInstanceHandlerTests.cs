using TodoApp.Features.Todos;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.CompleteTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.RecurringTodos;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.RecurringTodos;

public class CreateRecurringInstanceHandlerTests
{
    [Fact]
    public async Task Handle_CreatesNewActiveTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getHandler = new GetTodosHandler(db);
        var completeHandler = new CompleteTodoHandler(db);
        var recurHandler = new CreateRecurringInstanceHandler(db);

        var id = await addHandler.HandleAsync("Daily standup", recurrence: RecurrenceRule.Daily);
        var todos = await getHandler.HandleAsync();
        var todo = todos.First(t => t.Id == id);

        await recurHandler.HandleAsync(todo);

        var after = await getHandler.HandleAsync();
        Assert.Equal(2, after.Count);
        var newTodo = after.First(t => t.Id != id);
        Assert.Equal("Daily standup", newTodo.Title);
        Assert.False(newTodo.IsCompleted);
        Assert.Equal(RecurrenceRule.Daily, newTodo.Recurrence);
    }

    [Fact]
    public async Task Handle_PreservesPriorityAndNotes()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getHandler = new GetTodosHandler(db);
        var recurHandler = new CreateRecurringInstanceHandler(db);

        var id = await addHandler.HandleAsync("Weekly report", priority: TodoPriority.High, recurrence: RecurrenceRule.Weekly);
        var todos = await getHandler.HandleAsync();
        var todo = todos.First(t => t.Id == id);

        await recurHandler.HandleAsync(todo);

        var after = await getHandler.HandleAsync();
        var newTodo = after.First(t => t.Id != id);
        Assert.Equal(TodoPriority.High, newTodo.Priority);
        Assert.Equal(RecurrenceRule.Weekly, newTodo.Recurrence);
    }

    [Fact]
    public async Task Handle_AdvancesDueDateByInterval_Daily()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getHandler = new GetTodosHandler(db);
        var recurHandler = new CreateRecurringInstanceHandler(db);

        var dueDate = DateTime.Today.AddDays(3);
        var id = await addHandler.HandleAsync("Daily task", dueDate: dueDate, recurrence: RecurrenceRule.Daily);
        var todos = await getHandler.HandleAsync();
        var todo = todos.First(t => t.Id == id);

        await recurHandler.HandleAsync(todo);

        var after = await getHandler.HandleAsync();
        var newTodo = after.First(t => t.Id != id);
        Assert.Equal(dueDate.AddDays(1).Date, newTodo.DueDate!.Value.Date);
    }

    [Fact]
    public async Task Handle_AdvancesDueDateByInterval_Weekly()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getHandler = new GetTodosHandler(db);
        var recurHandler = new CreateRecurringInstanceHandler(db);

        var dueDate = DateTime.Today.AddDays(3);
        var id = await addHandler.HandleAsync("Weekly task", dueDate: dueDate, recurrence: RecurrenceRule.Weekly);
        var todos = await getHandler.HandleAsync();
        var todo = todos.First(t => t.Id == id);

        await recurHandler.HandleAsync(todo);

        var after = await getHandler.HandleAsync();
        var newTodo = after.First(t => t.Id != id);
        Assert.Equal(dueDate.AddDays(7).Date, newTodo.DueDate!.Value.Date);
    }

    [Fact]
    public async Task Handle_AdvancesDueDateByInterval_Monthly()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getHandler = new GetTodosHandler(db);
        var recurHandler = new CreateRecurringInstanceHandler(db);

        var dueDate = DateTime.Today.AddDays(5);
        var id = await addHandler.HandleAsync("Monthly task", dueDate: dueDate, recurrence: RecurrenceRule.Monthly);
        var todos = await getHandler.HandleAsync();
        var todo = todos.First(t => t.Id == id);

        await recurHandler.HandleAsync(todo);

        var after = await getHandler.HandleAsync();
        var newTodo = after.First(t => t.Id != id);
        Assert.Equal(dueDate.AddMonths(1).Date, newTodo.DueDate!.Value.Date);
    }

    [Fact]
    public async Task Handle_NoDueDate_NewTodoDueDateBasedOnToday()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getHandler = new GetTodosHandler(db);
        var recurHandler = new CreateRecurringInstanceHandler(db);

        var id = await addHandler.HandleAsync("No due date recurring", recurrence: RecurrenceRule.Weekly);
        var todos = await getHandler.HandleAsync();
        var todo = todos.First(t => t.Id == id);

        await recurHandler.HandleAsync(todo);

        var after = await getHandler.HandleAsync();
        var newTodo = after.First(t => t.Id != id);
        Assert.Equal(DateTime.Today.AddDays(7).Date, newTodo.DueDate!.Value.Date);
    }

    [Fact]
    public void Handle_ThrowsIfNoneRecurrence()
    {
        var todo = new TodoSummary(1, "Not recurring", false, DateTime.UtcNow, TodoPriority.None, null);
        Assert.Equal(RecurrenceRule.None, todo.Recurrence);
    }

    [Theory]
    [InlineData(RecurrenceRule.Daily, 1)]
    [InlineData(RecurrenceRule.Weekly, 7)]
    public void ComputeNextDueDate_AdvancesFromFutureDate(RecurrenceRule rule, int expectedDays)
    {
        var dueDate = DateTime.Today.AddDays(3);
        var next = CreateRecurringInstanceHandler.ComputeNextDueDate(dueDate, rule);
        Assert.Equal(dueDate.AddDays(expectedDays).Date, next!.Value.Date);
    }

    [Fact]
    public void ComputeNextDueDate_Monthly_AdvancesOneMonth()
    {
        var dueDate = DateTime.Today.AddDays(10); // future date to avoid overdue logic
        var next = CreateRecurringInstanceHandler.ComputeNextDueDate(dueDate, RecurrenceRule.Monthly);
        Assert.Equal(dueDate.AddMonths(1).Date, next!.Value.Date);
    }

    [Fact]
    public void ComputeNextDueDate_OverdueDateAdvancesFromToday()
    {
        var pastDueDate = DateTime.Today.AddDays(-5);
        var next = CreateRecurringInstanceHandler.ComputeNextDueDate(pastDueDate, RecurrenceRule.Daily);
        Assert.Equal(DateTime.Today.AddDays(1).Date, next!.Value.Date);
    }
}
