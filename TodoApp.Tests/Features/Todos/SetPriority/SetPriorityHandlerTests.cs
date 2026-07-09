using TodoApp.Features.Todos;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.SetPriority;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.SetPriority;

public class SetPriorityHandlerTests
{
    [Fact]
    public async Task SetPriority_UpdatesPriority()
    {
        var db = await TestDatabase.CreateAsync();
        var id = await new AddTodoHandler(db).HandleAsync("Test todo");

        await new SetPriorityHandler(db).HandleAsync(id, TodoPriority.High);

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Equal(TodoPriority.High, todos[0].Priority);
    }

    [Fact]
    public async Task SetPriority_CanSetAllValues()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var handler = new SetPriorityHandler(db);
        var getTodos = new GetTodosHandler(db);

        foreach (var priority in new[] { TodoPriority.Low, TodoPriority.Medium, TodoPriority.High, TodoPriority.None })
        {
            var id = await addHandler.HandleAsync($"Todo for {priority}");
            await handler.HandleAsync(id, priority);
            var todos = await getTodos.HandleAsync();
            Assert.Equal(priority, todos.First(t => t.Id == id).Priority);
        }
    }

    [Fact]
    public async Task SetPriority_ReplacesExistingPriority()
    {
        var db = await TestDatabase.CreateAsync();
        var id = await new AddTodoHandler(db).HandleAsync("Test todo", priority: TodoPriority.High);

        await new SetPriorityHandler(db).HandleAsync(id, TodoPriority.Low);

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Equal(TodoPriority.Low, todos[0].Priority);
    }

    [Fact]
    public async Task SetPriority_CanClearPriorityToNone()
    {
        var db = await TestDatabase.CreateAsync();
        var id = await new AddTodoHandler(db).HandleAsync("Test todo", priority: TodoPriority.Medium);

        await new SetPriorityHandler(db).HandleAsync(id, TodoPriority.None);

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Equal(TodoPriority.None, todos[0].Priority);
    }

    [Fact]
    public async Task SetPriority_NonExistentTodo_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            new SetPriorityHandler(db).HandleAsync(999, TodoPriority.High));
    }

    [Fact]
    public async Task SetPriority_OnlyAffectsTargetTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id1 = await addHandler.HandleAsync("Todo A", priority: TodoPriority.Low);
        var id2 = await addHandler.HandleAsync("Todo B", priority: TodoPriority.Low);

        await new SetPriorityHandler(db).HandleAsync(id1, TodoPriority.High);

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Equal(TodoPriority.Low, todos.First(t => t.Id == id2).Priority);
    }

    [Fact]
    public async Task SetPriority_DefaultIsNone()
    {
        var db = await TestDatabase.CreateAsync();
        await new AddTodoHandler(db).HandleAsync("Test todo");

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Equal(TodoPriority.None, todos[0].Priority);
    }
}
