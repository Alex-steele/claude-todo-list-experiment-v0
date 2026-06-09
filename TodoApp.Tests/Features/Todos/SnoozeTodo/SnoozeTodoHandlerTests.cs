using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.SnoozeTodo;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.SnoozeTodo;

public class SnoozeTodoHandlerTests
{
    [Fact]
    public async Task Snooze_UpdatesDueDate()
    {
        var db = await TestDatabase.CreateAsync();
        var id = await new AddTodoHandler(db).HandleAsync("Test todo", dueDate: DateTime.Today);

        await new SnoozeTodoHandler(db).HandleAsync(id, DateTime.Today.AddDays(7));

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Equal(DateTime.Today.AddDays(7).Date, todos[0].DueDate!.Value.Date);
    }

    [Fact]
    public async Task Snooze_SetsDueDateOnTodoWithNoPreviousDueDate()
    {
        var db = await TestDatabase.CreateAsync();
        var id = await new AddTodoHandler(db).HandleAsync("Test todo");

        var newDate = DateTime.Today.AddDays(3);
        await new SnoozeTodoHandler(db).HandleAsync(id, newDate);

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Equal(newDate.Date, todos[0].DueDate!.Value.Date);
    }

    [Fact]
    public async Task Snooze_NonExistentTodo_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            new SnoozeTodoHandler(db).HandleAsync(999, DateTime.Today.AddDays(1)));
    }

    [Fact]
    public async Task Snooze_DoesNotAffectOtherTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id1 = await addHandler.HandleAsync("Todo A");
        await addHandler.HandleAsync("Todo B");

        var newDate = DateTime.Today.AddDays(5);
        await new SnoozeTodoHandler(db).HandleAsync(id1, newDate);

        var todos = await new GetTodosHandler(db).HandleAsync();
        var other = todos.First(t => t.Title == "Todo B");
        Assert.Null(other.DueDate);
    }
}
