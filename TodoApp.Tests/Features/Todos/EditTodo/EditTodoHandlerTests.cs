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
}
