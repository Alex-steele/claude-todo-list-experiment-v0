using TodoApp.Features.Todos;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.EditTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.EditTodo;

public class EditTodoHandlerTests
{
    private readonly AddTodoHandler _addHandler;
    private readonly EditTodoHandler _editHandler;
    private readonly GetTodosHandler _getHandler;

    public EditTodoHandlerTests()
    {
        var db = TestDatabase.CreateAsync().GetAwaiter().GetResult();
        _addHandler = new AddTodoHandler(db);
        _editHandler = new EditTodoHandler(db);
        _getHandler = new GetTodosHandler(db);
    }

    [Fact]
    public async Task HandleAsync_ValidNewTitle_UpdatesTodoTitle()
    {
        var id = await _addHandler.HandleAsync("Original title");

        await _editHandler.HandleAsync(id, "Updated title");

        var todos = await _getHandler.HandleAsync();
        Assert.Equal("Updated title", todos.Single(t => t.Id == id).Title);
    }

    [Fact]
    public async Task HandleAsync_TitleWithWhitespace_IsTrimmedBeforeSaving()
    {
        var id = await _addHandler.HandleAsync("Original");

        await _editHandler.HandleAsync(id, "  Trimmed title  ");

        var todos = await _getHandler.HandleAsync();
        Assert.Equal("Trimmed title", todos.Single(t => t.Id == id).Title);
    }

    [Fact]
    public async Task HandleAsync_EmptyTitle_ThrowsArgumentException()
    {
        var id = await _addHandler.HandleAsync("Original");

        await Assert.ThrowsAsync<ArgumentException>(() => _editHandler.HandleAsync(id, ""));
    }

    [Fact]
    public async Task HandleAsync_WhitespaceTitle_ThrowsArgumentException()
    {
        var id = await _addHandler.HandleAsync("Original");

        await Assert.ThrowsAsync<ArgumentException>(() => _editHandler.HandleAsync(id, "   "));
    }

    [Fact]
    public async Task HandleAsync_NonExistentId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _editHandler.HandleAsync(9999, "New title"));
    }

    [Fact]
    public async Task HandleAsync_OnlyAffectsTargetTodo()
    {
        var id1 = await _addHandler.HandleAsync("Todo one");
        var id2 = await _addHandler.HandleAsync("Todo two");

        await _editHandler.HandleAsync(id1, "Todo one edited");

        var todos = await _getHandler.HandleAsync();
        Assert.Equal("Todo one edited", todos.Single(t => t.Id == id1).Title);
        Assert.Equal("Todo two", todos.Single(t => t.Id == id2).Title);
    }

    [Fact]
    public async Task HandleAsync_WithPriority_UpdatesPriority()
    {
        var id = await _addHandler.HandleAsync("Task", priority: TodoPriority.Low);

        await _editHandler.HandleAsync(id, "Task", priority: TodoPriority.High);

        var todos = await _getHandler.HandleAsync();
        Assert.Equal(TodoPriority.High, todos.Single(t => t.Id == id).Priority);
    }

    [Fact]
    public async Task HandleAsync_WithDueDate_UpdatesDueDate()
    {
        var id = await _addHandler.HandleAsync("Task");
        var newDue = DateTime.Today.AddDays(5);

        await _editHandler.HandleAsync(id, "Task", dueDate: newDue);

        var todos = await _getHandler.HandleAsync();
        Assert.Equal(newDue.Date, todos.Single(t => t.Id == id).DueDate!.Value.Date);
    }

    [Fact]
    public async Task HandleAsync_ClearDueDate_SetsDueDateToNull()
    {
        var id = await _addHandler.HandleAsync("Task", dueDate: DateTime.Today.AddDays(3));

        await _editHandler.HandleAsync(id, "Task", dueDate: null);

        var todos = await _getHandler.HandleAsync();
        Assert.Null(todos.Single(t => t.Id == id).DueDate);
    }

    [Fact]
    public async Task HandleAsync_NullPriority_DefaultsToNone()
    {
        var id = await _addHandler.HandleAsync("Task", priority: TodoPriority.High);

        await _editHandler.HandleAsync(id, "Task", priority: null);

        var todos = await _getHandler.HandleAsync();
        Assert.Equal(TodoPriority.None, todos.Single(t => t.Id == id).Priority);
    }

    [Fact]
    public async Task HandleAsync_AllFieldsTogether_UpdatesAll()
    {
        var id = await _addHandler.HandleAsync("Original");
        var newDue = DateTime.Today.AddDays(10);

        await _editHandler.HandleAsync(id, "Updated", TodoPriority.Medium, newDue);

        var todos = await _getHandler.HandleAsync();
        var todo = todos.Single(t => t.Id == id);
        Assert.Equal("Updated", todo.Title);
        Assert.Equal(TodoPriority.Medium, todo.Priority);
        Assert.Equal(newDue.Date, todo.DueDate!.Value.Date);
    }
}
