using TodoApp.Features.Todos;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.AddTodo;

public class AddTodoPriorityTests
{
    [Fact]
    public async Task HandleAsync_WithoutPriority_DefaultsToNone()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        await addHandler.HandleAsync("Task with no priority");
        var todos = await getHandler.HandleAsync();

        Assert.Equal(TodoPriority.None, todos[0].Priority);
    }

    [Theory]
    [InlineData(TodoPriority.Low)]
    [InlineData(TodoPriority.Medium)]
    [InlineData(TodoPriority.High)]
    public async Task HandleAsync_WithPriority_PersistsPriorityCorrectly(TodoPriority priority)
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        await addHandler.HandleAsync("Task", priority);
        var todos = await getHandler.HandleAsync();

        Assert.Equal(priority, todos[0].Priority);
    }

    [Fact]
    public async Task HandleAsync_MultiplePriorities_EachStoredIndependently()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        var idHigh = await addHandler.HandleAsync("High priority task", TodoPriority.High);
        var idLow = await addHandler.HandleAsync("Low priority task", TodoPriority.Low);

        var todos = await getHandler.HandleAsync();
        var high = todos.First(t => t.Id == idHigh);
        var low = todos.First(t => t.Id == idLow);

        Assert.Equal(TodoPriority.High, high.Priority);
        Assert.Equal(TodoPriority.Low, low.Priority);
    }
}
