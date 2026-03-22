using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.DeleteTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.DeleteTodo;

public class DeleteTodoHandlerTests
{
    [Fact]
    public async Task HandleAsync_ExistingTodo_RemovesIt()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var deleteHandler = new DeleteTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id = await addHandler.HandleAsync("Todo to delete");
        await deleteHandler.HandleAsync(id);

        var todos = await getHandler.HandleAsync();
        Assert.Empty(todos);
    }

    [Fact]
    public async Task HandleAsync_NonExistentId_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new DeleteTodoHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(999));
    }

    [Fact]
    public async Task HandleAsync_OnlyDeletesTargetTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var deleteHandler = new DeleteTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id1 = await addHandler.HandleAsync("Keep this");
        var id2 = await addHandler.HandleAsync("Delete this");
        await deleteHandler.HandleAsync(id2);

        var todos = await getHandler.HandleAsync();
        Assert.Single(todos);
        Assert.Equal(id1, todos[0].Id);
    }

    [Fact]
    public async Task HandleAsync_CompletedTodo_DeletesSuccessfully()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var deleteHandler = new DeleteTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id = await addHandler.HandleAsync("Completed todo");
        await deleteHandler.HandleAsync(id);

        var todos = await getHandler.HandleAsync();
        Assert.Empty(todos);
    }
}
