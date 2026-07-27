using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.TimeTracking;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.TimeTracking;

public class StartTimerHandlerTests
{
    [Fact]
    public async Task HandleAsync_SetsTimerStartedAt()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var startHandler = new StartTimerHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id = await addHandler.HandleAsync("Focus task");
        await startHandler.HandleAsync(id);

        var todos = await getHandler.HandleAsync();
        Assert.NotNull(todos.Single(t => t.Id == id).TimerStartedAt);
    }

    [Fact]
    public async Task HandleAsync_NonExistentId_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        var startHandler = new StartTimerHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() => startHandler.HandleAsync(999));
    }

    [Fact]
    public async Task HandleAsync_AnotherTimerAlreadyRunning_StopsItAndAccumulatesItsElapsedTime()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var startHandler = new StartTimerHandler(db);
        var getHandler = new GetTodosHandler(db);

        var firstId = await addHandler.HandleAsync("First task");
        var secondId = await addHandler.HandleAsync("Second task");

        await startHandler.HandleAsync(firstId);
        await Task.Delay(1100);
        await startHandler.HandleAsync(secondId);

        var todos = await getHandler.HandleAsync();
        var first = todos.Single(t => t.Id == firstId);
        var second = todos.Single(t => t.Id == secondId);

        Assert.Null(first.TimerStartedAt);
        Assert.True(first.TimeSpentSeconds >= 1);
        Assert.NotNull(second.TimerStartedAt);
        Assert.Equal(0, second.TimeSpentSeconds);
    }

    [Fact]
    public async Task HandleAsync_RestartingSameTodo_KeepsTimerRunningWithoutDoubleCounting()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var startHandler = new StartTimerHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id = await addHandler.HandleAsync("Only task");
        await startHandler.HandleAsync(id);
        await startHandler.HandleAsync(id);

        var todos = await getHandler.HandleAsync();
        var todo = todos.Single(t => t.Id == id);
        Assert.NotNull(todo.TimerStartedAt);
        Assert.Equal(0, todo.TimeSpentSeconds);
    }
}
