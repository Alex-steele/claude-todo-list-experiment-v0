using TodoApp.Features.Todos;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.MultiAdd;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.MultiAdd;

public class AddMultipleTodosHandlerTests
{
    [Fact]
    public async Task AddMultiple_ValidTitles_AllCreated()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new AddMultipleTodosHandler(db);
        var getTodos = new GetTodosHandler(db);

        var count = await handler.HandleAsync(["Buy milk", "Call dentist", "Pay bills"]);

        Assert.Equal(3, count);
        var todos = await getTodos.HandleAsync();
        Assert.Equal(3, todos.Count);
        Assert.Contains(todos, t => t.Title == "Buy milk");
        Assert.Contains(todos, t => t.Title == "Call dentist");
        Assert.Contains(todos, t => t.Title == "Pay bills");
    }

    [Fact]
    public async Task AddMultiple_EmptyList_ReturnsZero()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new AddMultipleTodosHandler(db);
        var getTodos = new GetTodosHandler(db);

        var count = await handler.HandleAsync([]);

        Assert.Equal(0, count);
        var todos = await getTodos.HandleAsync();
        Assert.Empty(todos);
    }

    [Fact]
    public async Task AddMultiple_TrimsWhitespace()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new AddMultipleTodosHandler(db);
        var getTodos = new GetTodosHandler(db);

        await handler.HandleAsync(["  Buy milk  ", "\tCall dentist\t"]);

        var todos = await getTodos.HandleAsync();
        Assert.Contains(todos, t => t.Title == "Buy milk");
        Assert.Contains(todos, t => t.Title == "Call dentist");
    }

    [Fact]
    public async Task AddMultiple_SkipsBlankLines()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new AddMultipleTodosHandler(db);
        var getTodos = new GetTodosHandler(db);

        var count = await handler.HandleAsync(["Buy milk", "", "  ", "Call dentist"]);

        Assert.Equal(2, count);
        var todos = await getTodos.HandleAsync();
        Assert.Equal(2, todos.Count);
    }

    [Fact]
    public async Task AddMultiple_AllBlank_ReturnsZero()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new AddMultipleTodosHandler(db);
        var getTodos = new GetTodosHandler(db);

        var count = await handler.HandleAsync(["", "  ", "\t"]);

        Assert.Equal(0, count);
        var todos = await getTodos.HandleAsync();
        Assert.Empty(todos);
    }

    [Fact]
    public async Task AddMultiple_UsesSpecifiedListId()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new AddMultipleTodosHandler(db);
        var getTodos = new GetTodosHandler(db);

        await handler.HandleAsync(["Task A", "Task B"], listId: 1);

        var todos = await getTodos.HandleAsync();
        Assert.All(todos, t => Assert.Equal(1, t.ListId));
    }

    [Fact]
    public async Task AddMultiple_SingleTitle_CreatesOneTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new AddMultipleTodosHandler(db);
        var getTodos = new GetTodosHandler(db);

        var count = await handler.HandleAsync(["Just one task"]);

        Assert.Equal(1, count);
        var todos = await getTodos.HandleAsync();
        Assert.Single(todos);
        Assert.Equal("Just one task", todos[0].Title);
    }

    [Fact]
    public async Task AddMultiple_DefaultPriorityIsNone()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new AddMultipleTodosHandler(db);
        var getTodos = new GetTodosHandler(db);

        await handler.HandleAsync(["Task A"]);

        var todos = await getTodos.HandleAsync();
        Assert.Equal(TodoPriority.None, todos[0].Priority);
    }
}
