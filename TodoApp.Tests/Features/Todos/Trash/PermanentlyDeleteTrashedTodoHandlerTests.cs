using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.DeleteTodo;
using TodoApp.Features.Todos.Trash;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Trash;

public class PermanentlyDeleteTrashedTodoHandlerTests
{
    [Fact]
    public async Task HandleAsync_RemovesEntryFromTrash()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var delete = new DeleteTodoHandler(db);
        var getTrashed = new GetTrashedTodosHandler(db);
        var permanentlyDelete = new PermanentlyDeleteTrashedTodoHandler(db);

        var id = await add.HandleAsync("Gone for good");
        await delete.HandleAsync(id);
        var trashId = (await getTrashed.HandleAsync()).Single().TrashId;

        await permanentlyDelete.HandleAsync(trashId);

        Assert.Empty(await getTrashed.HandleAsync());
    }

    [Fact]
    public async Task HandleAsync_NonExistentTrashId_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new PermanentlyDeleteTrashedTodoHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(999));
    }

    [Fact]
    public async Task HandleAsync_OnlyDeletesTargetEntry()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var delete = new DeleteTodoHandler(db);
        var getTrashed = new GetTrashedTodosHandler(db);
        var permanentlyDelete = new PermanentlyDeleteTrashedTodoHandler(db);

        var id1 = await add.HandleAsync("Keep in trash");
        await delete.HandleAsync(id1);
        var id2 = await add.HandleAsync("Remove from trash");
        await delete.HandleAsync(id2);

        var toRemove = (await getTrashed.HandleAsync()).Single(t => t.Title == "Remove from trash");
        await permanentlyDelete.HandleAsync(toRemove.TrashId);

        var remaining = await getTrashed.HandleAsync();
        Assert.Single(remaining);
        Assert.Equal("Keep in trash", remaining[0].Title);
    }
}
