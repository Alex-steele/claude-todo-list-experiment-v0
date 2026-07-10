using TodoApp.Features.Todos;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.DeleteTodo;
using TodoApp.Features.Todos.Trash;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Trash;

public class GetTrashedTodosHandlerTests
{
    [Fact]
    public async Task HandleAsync_NoDeletions_ReturnsEmpty()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new GetTrashedTodosHandler(db);

        var result = await handler.HandleAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task HandleAsync_AfterDelete_ReturnsTrashedTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var delete = new DeleteTodoHandler(db);
        var handler = new GetTrashedTodosHandler(db);

        var dueDate = DateTime.Today.AddDays(2);
        var id = await add.HandleAsync("Buy milk", TodoPriority.High, dueDate);
        await delete.HandleAsync(id);

        var trashed = await handler.HandleAsync();

        var entry = Assert.Single(trashed);
        Assert.Equal("Buy milk", entry.Title);
        Assert.Equal(TodoPriority.High, entry.Priority);
        Assert.Equal(dueDate.Date, entry.DueDate?.Date);
        Assert.False(entry.IsCompleted);
    }

    [Fact]
    public async Task HandleAsync_MultipleDeletions_OrderedNewestFirst()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var delete = new DeleteTodoHandler(db);
        var handler = new GetTrashedTodosHandler(db);

        var id1 = await add.HandleAsync("First deleted");
        await delete.HandleAsync(id1);
        var id2 = await add.HandleAsync("Second deleted");
        await delete.HandleAsync(id2);

        var trashed = await handler.HandleAsync();

        Assert.Equal(2, trashed.Count);
        Assert.Equal("Second deleted", trashed[0].Title);
        Assert.Equal("First deleted", trashed[1].Title);
    }

    [Fact]
    public async Task HandleAsync_DoesNotAffectActiveTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var delete = new DeleteTodoHandler(db);
        var handler = new GetTrashedTodosHandler(db);

        var keepId = await add.HandleAsync("Keep this");
        var deleteId = await add.HandleAsync("Trash this");
        await delete.HandleAsync(deleteId);

        var trashed = await handler.HandleAsync();

        Assert.Single(trashed);
        Assert.Equal("Trash this", trashed[0].Title);
    }
}
