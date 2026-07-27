using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.TimeTracking;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.TimeTracking;

public class StopTimerHandlerTests
{
    [Fact]
    public async Task HandleAsync_RunningTimer_AccumulatesElapsedTimeAndClearsTimerStartedAt()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var startHandler = new StartTimerHandler(db);
        var stopHandler = new StopTimerHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id = await addHandler.HandleAsync("Focus task");
        await startHandler.HandleAsync(id);
        await Task.Delay(1100);
        await stopHandler.HandleAsync(id);

        var todo = (await getHandler.HandleAsync()).Single(t => t.Id == id);
        Assert.Null(todo.TimerStartedAt);
        Assert.True(todo.TimeSpentSeconds >= 1);
    }

    [Fact]
    public async Task HandleAsync_NoTimerRunning_DoesNothing()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var stopHandler = new StopTimerHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id = await addHandler.HandleAsync("Untouched task");
        await stopHandler.HandleAsync(id);

        var todo = (await getHandler.HandleAsync()).Single(t => t.Id == id);
        Assert.Null(todo.TimerStartedAt);
        Assert.Equal(0, todo.TimeSpentSeconds);
    }

    [Fact]
    public async Task HandleAsync_StoppingTwiceInARow_DoesNotDoubleCountElapsedTime()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var startHandler = new StartTimerHandler(db);
        var stopHandler = new StopTimerHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id = await addHandler.HandleAsync("Focus task");
        await startHandler.HandleAsync(id);
        await Task.Delay(1100);
        await stopHandler.HandleAsync(id);

        var afterFirstStop = (await getHandler.HandleAsync()).Single(t => t.Id == id).TimeSpentSeconds;
        await stopHandler.HandleAsync(id);
        var afterSecondStop = (await getHandler.HandleAsync()).Single(t => t.Id == id).TimeSpentSeconds;

        Assert.Equal(afterFirstStop, afterSecondStop);
    }
}
