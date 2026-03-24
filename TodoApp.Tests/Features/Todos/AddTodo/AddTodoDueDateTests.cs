using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.AddTodo;

public class AddTodoDueDateTests
{
    [Fact]
    public async Task AddTodo_WithDueDate_PersistsCorrectly()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getHandler = new GetTodosHandler(db);
        var dueDate = new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Utc);

        await addHandler.HandleAsync("Task with due date", dueDate: dueDate);
        var todos = await getHandler.HandleAsync();

        Assert.Single(todos);
        Assert.NotNull(todos[0].DueDate);
        Assert.Equal(dueDate.Date, todos[0].DueDate!.Value.Date);
    }

    [Fact]
    public async Task AddTodo_WithoutDueDate_ReturnsNullDueDate()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        await addHandler.HandleAsync("Task without due date");
        var todos = await getHandler.HandleAsync();

        Assert.Single(todos);
        Assert.Null(todos[0].DueDate);
    }

    [Fact]
    public async Task AddTodo_WithPastDueDate_PersistsCorrectly()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getHandler = new GetTodosHandler(db);
        var pastDueDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        await addHandler.HandleAsync("Overdue task", dueDate: pastDueDate);
        var todos = await getHandler.HandleAsync();

        Assert.Single(todos);
        Assert.NotNull(todos[0].DueDate);
        Assert.Equal(pastDueDate.Date, todos[0].DueDate!.Value.Date);
    }
}
