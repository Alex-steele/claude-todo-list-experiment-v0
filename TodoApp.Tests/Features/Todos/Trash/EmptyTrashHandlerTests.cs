using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.DeleteTodo;
using TodoApp.Features.Todos.Trash;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Trash;

public class EmptyTrashHandlerTests
{
    [Fact]
    public async Task HandleAsync_EmptyTrash_ReturnsZero()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new EmptyTrashHandler(db);

        var count = await handler.HandleAsync();

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task HandleAsync_RemovesAllTrashedTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var delete = new DeleteTodoHandler(db);
        var getTrashed = new GetTrashedTodosHandler(db);
        var handler = new EmptyTrashHandler(db);

        var id1 = await add.HandleAsync("Trash A");
        var id2 = await add.HandleAsync("Trash B");
        await delete.HandleAsync(id1);
        await delete.HandleAsync(id2);

        var count = await handler.HandleAsync();

        Assert.Equal(2, count);
        Assert.Empty(await getTrashed.HandleAsync());
    }

    [Fact]
    public async Task HandleAsync_DoesNotAffectActiveTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var delete = new DeleteTodoHandler(db);
        var handler = new EmptyTrashHandler(db);
        var getTodos = new TodoApp.Features.Todos.GetTodos.GetTodosHandler(db);

        var keepId = await add.HandleAsync("Keep this active");
        var trashId = await add.HandleAsync("Trash this");
        await delete.HandleAsync(trashId);

        await handler.HandleAsync();

        var todos = await getTodos.HandleAsync();
        Assert.Single(todos);
        Assert.Equal(keepId, todos[0].Id);
    }
}
