using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.BulkOperations;
using TodoApp.Features.Todos.CompleteTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.BulkOperations;

public class BulkOperationsHandlerTests
{
    [Fact]
    public async Task CompleteAsync_MarksAllSelectedTodosComplete()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id1 = await addHandler.HandleAsync("Todo 1");
        var id2 = await addHandler.HandleAsync("Todo 2");
        var id3 = await addHandler.HandleAsync("Todo 3");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.CompleteAsync([id1, id2]);

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        Assert.True(todos.First(t => t.Id == id1).IsCompleted);
        Assert.True(todos.First(t => t.Id == id2).IsCompleted);
        Assert.False(todos.First(t => t.Id == id3).IsCompleted);
    }

    [Fact]
    public async Task DeleteAsync_RemovesAllSelectedTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id1 = await addHandler.HandleAsync("Todo 1");
        var id2 = await addHandler.HandleAsync("Todo 2");
        var id3 = await addHandler.HandleAsync("Todo 3");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.DeleteAsync([id1, id2]);

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        var remaining = Assert.Single(todos);
        Assert.Equal(id3, remaining.Id);
    }

    [Fact]
    public async Task CompleteAsync_WithEmptyList_DoesNothing()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Todo 1");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.CompleteAsync([]); // Should not throw

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        Assert.False(todos[0].IsCompleted);
    }

    [Fact]
    public async Task DeleteAsync_WithEmptyList_DoesNothing()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Todo 1");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.DeleteAsync([]); // Should not throw

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        Assert.Single(todos);
    }

    [Fact]
    public async Task CompleteAsync_AlreadyCompletedTodo_RemainsComplete()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var completeHandler = new CompleteTodoHandler(db);
        var id = await addHandler.HandleAsync("Todo 1");
        await completeHandler.HandleAsync(id); // Toggle to completed

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.CompleteAsync([id]); // Complete again via bulk

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        Assert.True(todos[0].IsCompleted);
    }

    [Fact]
    public async Task CompleteAsync_OnlyAffectsSpecifiedTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id1 = await addHandler.HandleAsync("Target");
        var id2 = await addHandler.HandleAsync("Bystander");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.CompleteAsync([id1]);

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        Assert.True(todos.First(t => t.Id == id1).IsCompleted);
        Assert.False(todos.First(t => t.Id == id2).IsCompleted);
    }
}
