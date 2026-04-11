using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.ClearCompleted;
using TodoApp.Features.Todos.CompleteTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.ClearCompleted;

public class ClearCompletedHandlerTests
{
    [Fact]
    public async Task ClearCompleted_NoCompletedTodos_ReturnsEmpty()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new ClearCompletedHandler(db);

        await add.HandleAsync("Active task");

        var result = await handler.HandleAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task ClearCompleted_RemovesAllCompletedTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var complete = new CompleteTodoHandler(db);
        var handler = new ClearCompletedHandler(db);
        var get = new GetTodosHandler(db);

        var id1 = await add.HandleAsync("Task A");
        var id2 = await add.HandleAsync("Task B");
        await complete.HandleAsync(id1);
        await complete.HandleAsync(id2);

        await handler.HandleAsync();

        var remaining = await get.HandleAsync();
        Assert.Empty(remaining);
    }

    [Fact]
    public async Task ClearCompleted_OnlyRemovesCompleted_LeavesActiveTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var complete = new CompleteTodoHandler(db);
        var handler = new ClearCompletedHandler(db);
        var get = new GetTodosHandler(db);

        var activeId = await add.HandleAsync("Still active");
        var doneId = await add.HandleAsync("Already done");
        await complete.HandleAsync(doneId);

        await handler.HandleAsync();

        var remaining = await get.HandleAsync();
        Assert.Single(remaining);
        Assert.Equal(activeId, remaining[0].Id);
    }

    [Fact]
    public async Task ClearCompleted_ReturnsSnapshotOfDeletedTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var complete = new CompleteTodoHandler(db);
        var handler = new ClearCompletedHandler(db);

        var id1 = await add.HandleAsync("Task A");
        var id2 = await add.HandleAsync("Task B");
        await complete.HandleAsync(id1);
        await complete.HandleAsync(id2);

        var snapshot = await handler.HandleAsync();

        Assert.Equal(2, snapshot.Count);
        Assert.All(snapshot, t => Assert.True(t.IsCompleted));
    }

    [Fact]
    public async Task ClearCompleted_EmptyDatabase_ReturnsEmpty()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new ClearCompletedHandler(db);

        var result = await handler.HandleAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task ClearCompleted_SnapshotPreservesAllProperties()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var complete = new CompleteTodoHandler(db);
        var handler = new ClearCompletedHandler(db);

        var dueDate = DateTime.Today.AddDays(3);
        var id = await add.HandleAsync("Important task", TodoApp.Features.Todos.TodoPriority.High, dueDate);
        await complete.HandleAsync(id);

        var snapshot = await handler.HandleAsync();

        var restored = snapshot.Single();
        Assert.Equal("Important task", restored.Title);
        Assert.Equal(TodoApp.Features.Todos.TodoPriority.High, restored.Priority);
        Assert.Equal(dueDate.Date, restored.DueDate?.Date);
        Assert.True(restored.IsCompleted);
    }
}
