using TodoApp.Features.Todos;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.DuplicateTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.RecurringTodos;
using TodoApp.Features.Todos.TimeEstimates;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.DuplicateTodo;

public class DuplicateTodoHandlerTests
{
    [Fact]
    public async Task Duplicate_CreatesNewTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var dupHandler = new DuplicateTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id = await addHandler.HandleAsync("Original");
        await dupHandler.HandleAsync(id);

        var todos = await getHandler.HandleAsync();
        Assert.Equal(2, todos.Count);
    }

    [Fact]
    public async Task Duplicate_AppendsCopySuffix()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var dupHandler = new DuplicateTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id = await addHandler.HandleAsync("Buy milk");
        await dupHandler.HandleAsync(id);

        var todos = await getHandler.HandleAsync();
        Assert.Contains(todos, t => t.Title == "Buy milk (copy)");
    }

    [Fact]
    public async Task Duplicate_CopiesPriority()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var dupHandler = new DuplicateTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id = await addHandler.HandleAsync("Task", priority: TodoPriority.High);
        await dupHandler.HandleAsync(id);

        var todos = await getHandler.HandleAsync();
        var copy = todos.Single(t => t.Title.EndsWith("(copy)"));
        Assert.Equal(TodoPriority.High, copy.Priority);
    }

    [Fact]
    public async Task Duplicate_CopiesDueDate()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var dupHandler = new DuplicateTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        var due = DateTime.Today.AddDays(7);
        var id = await addHandler.HandleAsync("Task", dueDate: due);
        await dupHandler.HandleAsync(id);

        var todos = await getHandler.HandleAsync();
        var copy = todos.Single(t => t.Title.EndsWith("(copy)"));
        Assert.Equal(due.Date, copy.DueDate!.Value.Date);
    }

    [Fact]
    public async Task Duplicate_CopiesRecurrence()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var dupHandler = new DuplicateTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id = await addHandler.HandleAsync("Task", recurrence: RecurrenceRule.Weekly);
        await dupHandler.HandleAsync(id);

        var todos = await getHandler.HandleAsync();
        var copy = todos.Single(t => t.Title.EndsWith("(copy)"));
        Assert.Equal(RecurrenceRule.Weekly, copy.Recurrence);
    }

    [Fact]
    public async Task Duplicate_CopiesTimeEstimate()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var dupHandler = new DuplicateTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id = await addHandler.HandleAsync("Task", timeEstimate: TimeEstimate.OneHour);
        await dupHandler.HandleAsync(id);

        var todos = await getHandler.HandleAsync();
        var copy = todos.Single(t => t.Title.EndsWith("(copy)"));
        Assert.Equal(TimeEstimate.OneHour, copy.TimeEstimate);
    }

    [Fact]
    public async Task Duplicate_StartsIncomplete()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var dupHandler = new DuplicateTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id = await addHandler.HandleAsync("Task");
        await dupHandler.HandleAsync(id);

        var todos = await getHandler.HandleAsync();
        var copy = todos.Single(t => t.Title.EndsWith("(copy)"));
        Assert.False(copy.IsCompleted);
        Assert.Null(copy.CompletedAt);
    }

    [Fact]
    public async Task Duplicate_StartsUnpinned()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var dupHandler = new DuplicateTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id = await addHandler.HandleAsync("Task");
        await dupHandler.HandleAsync(id);

        var todos = await getHandler.HandleAsync();
        var copy = todos.Single(t => t.Title.EndsWith("(copy)"));
        Assert.False(copy.IsPinned);
    }

    [Fact]
    public async Task Duplicate_OriginalUnchanged()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var dupHandler = new DuplicateTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id = await addHandler.HandleAsync("Original", priority: TodoPriority.Medium);
        await dupHandler.HandleAsync(id);

        var todos = await getHandler.HandleAsync();
        var original = todos.Single(t => t.Id == id);
        Assert.Equal("Original", original.Title);
        Assert.Equal(TodoPriority.Medium, original.Priority);
    }

    [Fact]
    public async Task Duplicate_NonExistentId_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new DuplicateTodoHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(99999));
    }

    [Fact]
    public async Task Duplicate_ReturnsNewId()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var dupHandler = new DuplicateTodoHandler(db);

        var originalId = await addHandler.HandleAsync("Task");
        var newId = await dupHandler.HandleAsync(originalId);

        Assert.NotEqual(originalId, newId);
    }
}
