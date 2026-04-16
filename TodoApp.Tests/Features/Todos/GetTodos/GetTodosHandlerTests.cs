using TodoApp.Features.Todos.AddTodo;
using Xunit;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Tests.Infrastructure;

namespace TodoApp.Tests.Features.Todos.GetTodos;

public class GetTodosHandlerTests
{
    [Fact]
    public async Task HandleAsync_EmptyDatabase_ReturnsEmptyList()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new GetTodosHandler(db);

        var todos = await handler.HandleAsync();

        Assert.Empty(todos);
    }

    [Fact]
    public async Task HandleAsync_WithTodos_ReturnsInSortOrderAsc()
    {
        // GetTodosHandler now returns todos ordered by SortOrder ASC (insertion order)
        // so they arrive ready for Manual sort. FilterSortTodosHandler re-sorts for other modes.
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        await addHandler.HandleAsync("First");
        await addHandler.HandleAsync("Second");

        var todos = await getHandler.HandleAsync();

        Assert.Equal(2, todos.Count);
        Assert.Equal("First", todos[0].Title);
        Assert.Equal("Second", todos[1].Title);
    }

    [Fact]
    public async Task HandleAsync_NewTodo_IsNotCompleted()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        await addHandler.HandleAsync("Test todo");
        var todos = await getHandler.HandleAsync();

        Assert.False(todos[0].IsCompleted);
    }
}
