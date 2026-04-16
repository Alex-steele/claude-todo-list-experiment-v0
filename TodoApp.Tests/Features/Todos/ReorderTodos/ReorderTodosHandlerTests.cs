using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.ReorderTodos;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.ReorderTodos;

public class ReorderTodosHandlerTests
{
    [Fact]
    public async Task Reorder_ChangesOrder()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var reorderHandler = new ReorderTodosHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id1 = await addHandler.HandleAsync("First");
        var id2 = await addHandler.HandleAsync("Second");
        var id3 = await addHandler.HandleAsync("Third");

        // Reverse the order
        await reorderHandler.HandleAsync([id3, id2, id1]);

        var todos = await getHandler.HandleAsync();
        Assert.Equal(id3, todos[0].Id);
        Assert.Equal(id2, todos[1].Id);
        Assert.Equal(id1, todos[2].Id);
    }

    [Fact]
    public async Task Reorder_EmptyList_DoesNothing()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new ReorderTodosHandler(db);

        // Should not throw
        await handler.HandleAsync([]);
    }

    [Fact]
    public async Task Reorder_SingleItem_Succeeds()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var reorderHandler = new ReorderTodosHandler(db);

        var id = await addHandler.HandleAsync("Only task");

        await reorderHandler.HandleAsync([id]);
    }

    [Fact]
    public async Task Reorder_PreservesExistingData()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var reorderHandler = new ReorderTodosHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id1 = await addHandler.HandleAsync("Alpha");
        var id2 = await addHandler.HandleAsync("Beta");

        await reorderHandler.HandleAsync([id2, id1]);

        var todos = await getHandler.HandleAsync();
        Assert.Equal("Beta", todos[0].Title);
        Assert.Equal("Alpha", todos[1].Title);
    }
}
