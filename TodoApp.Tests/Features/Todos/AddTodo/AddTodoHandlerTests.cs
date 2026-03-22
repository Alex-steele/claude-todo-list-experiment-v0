using TodoApp.Features.Todos.AddTodo;
using Xunit;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Tests.Infrastructure;

namespace TodoApp.Tests.Features.Todos.AddTodo;

public class AddTodoHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidTitle_ReturnsPositiveId()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new AddTodoHandler(db);

        var id = await handler.HandleAsync("Buy groceries");

        Assert.True(id > 0);
    }

    [Fact]
    public async Task HandleAsync_EmptyTitle_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new AddTodoHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(""));
    }

    [Fact]
    public async Task HandleAsync_WhitespaceTitle_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new AddTodoHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync("   "));
    }

    [Fact]
    public async Task HandleAsync_TitleWithWhitespace_IsTrimmedBeforeSaving()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        await addHandler.HandleAsync("  Buy groceries  ");
        var todos = await getHandler.HandleAsync();

        Assert.Equal("Buy groceries", todos[0].Title);
    }

    [Fact]
    public async Task HandleAsync_MultipleTodos_EachGetUniqueId()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new AddTodoHandler(db);

        var id1 = await handler.HandleAsync("First");
        var id2 = await handler.HandleAsync("Second");

        Assert.NotEqual(id1, id2);
    }
}
