using TodoApp.Features.Todos.AddTodo;
using Xunit;
using TodoApp.Features.Todos.CompleteTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Tests.Infrastructure;

namespace TodoApp.Tests.Features.Todos.CompleteTodo;

public class CompleteTodoHandlerTests
{
    [Fact]
    public async Task HandleAsync_IncompleteTodo_MarksAsCompleted()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var completeHandler = new CompleteTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id = await addHandler.HandleAsync("Test todo");
        await completeHandler.HandleAsync(id);

        var todos = await getHandler.HandleAsync();
        Assert.True(todos[0].IsCompleted);
    }

    [Fact]
    public async Task HandleAsync_CompletedTodo_TogglesBackToIncomplete()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var completeHandler = new CompleteTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id = await addHandler.HandleAsync("Test todo");
        await completeHandler.HandleAsync(id);
        await completeHandler.HandleAsync(id);

        var todos = await getHandler.HandleAsync();
        Assert.False(todos[0].IsCompleted);
    }

    [Fact]
    public async Task HandleAsync_NonExistentId_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new CompleteTodoHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(999));
    }

    [Fact]
    public async Task HandleAsync_OnlyAffectsTargetTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var completeHandler = new CompleteTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id1 = await addHandler.HandleAsync("Todo 1");
        var id2 = await addHandler.HandleAsync("Todo 2");
        await completeHandler.HandleAsync(id1);

        var todos = await getHandler.HandleAsync();
        var todo1 = todos.First(t => t.Id == id1);
        var todo2 = todos.First(t => t.Id == id2);

        Assert.True(todo1.IsCompleted);
        Assert.False(todo2.IsCompleted);
    }
}
