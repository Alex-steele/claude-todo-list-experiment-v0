using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.SetDueDate;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.SetDueDate;

public class SetDueDateHandlerTests
{
    [Fact]
    public async Task SetDueDate_UpdatesDueDate()
    {
        var db = await TestDatabase.CreateAsync();
        var id = await new AddTodoHandler(db).HandleAsync("Test todo");
        var newDate = DateTime.Today.AddDays(3);

        await new SetDueDateHandler(db).HandleAsync(id, newDate);

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Equal(newDate.Date, todos[0].DueDate!.Value.Date);
    }

    [Fact]
    public async Task SetDueDate_ClearsDueDate_WhenNullPassed()
    {
        var db = await TestDatabase.CreateAsync();
        var id = await new AddTodoHandler(db).HandleAsync("Test todo", dueDate: DateTime.Today);

        await new SetDueDateHandler(db).HandleAsync(id, null);

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Null(todos[0].DueDate);
    }

    [Fact]
    public async Task SetDueDate_ReplacesExistingDueDate()
    {
        var db = await TestDatabase.CreateAsync();
        var originalDate = DateTime.Today;
        var id = await new AddTodoHandler(db).HandleAsync("Test todo", dueDate: originalDate);
        var newDate = DateTime.Today.AddDays(7);

        await new SetDueDateHandler(db).HandleAsync(id, newDate);

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Equal(newDate.Date, todos[0].DueDate!.Value.Date);
    }

    [Fact]
    public async Task SetDueDate_NonExistentTodo_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            new SetDueDateHandler(db).HandleAsync(999, DateTime.Today));
    }

    [Fact]
    public async Task SetDueDate_DoesNotAffectOtherTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id1 = await addHandler.HandleAsync("Todo A");
        await addHandler.HandleAsync("Todo B", dueDate: DateTime.Today);

        await new SetDueDateHandler(db).HandleAsync(id1, DateTime.Today.AddDays(10));

        var todos = await new GetTodosHandler(db).HandleAsync();
        var todoBDate = todos.First(t => t.Title == "Todo B").DueDate;
        Assert.Equal(DateTime.Today.Date, todoBDate!.Value.Date);
    }
}
