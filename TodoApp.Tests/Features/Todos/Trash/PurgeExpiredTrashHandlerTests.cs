using Dapper;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.DeleteTodo;
using TodoApp.Features.Todos.Trash;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Trash;

public class PurgeExpiredTrashHandlerTests
{
    [Fact]
    public async Task HandleAsync_EmptyTrash_ReturnsZero()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new PurgeExpiredTrashHandler(db);

        var count = await handler.HandleAsync();

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task HandleAsync_RecentlyDeletedTodo_IsNotPurged()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var delete = new DeleteTodoHandler(db);
        var getTrashed = new GetTrashedTodosHandler(db);
        var handler = new PurgeExpiredTrashHandler(db);

        var id = await add.HandleAsync("Recently trashed");
        await delete.HandleAsync(id);

        var count = await handler.HandleAsync();

        Assert.Equal(0, count);
        Assert.Single(await getTrashed.HandleAsync());
    }

    [Fact]
    public async Task HandleAsync_TodoOlderThanRetention_IsPurged()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var delete = new DeleteTodoHandler(db);
        var getTrashed = new GetTrashedTodosHandler(db);
        var handler = new PurgeExpiredTrashHandler(db);

        var id = await add.HandleAsync("Ancient trash");
        await delete.HandleAsync(id);

        using (var conn = db.CreateConnection())
        {
            var oldDate = DateTime.UtcNow.AddDays(-(PurgeExpiredTrashHandler.RetentionDays + 1)).ToString("O");
            await conn.ExecuteAsync("UPDATE DeletedTodos SET DeletedAt = @DeletedAt", new { DeletedAt = oldDate });
        }

        var count = await handler.HandleAsync();

        Assert.Equal(1, count);
        Assert.Empty(await getTrashed.HandleAsync());
    }

    [Fact]
    public async Task HandleAsync_MixOfOldAndRecent_OnlyPurgesOld()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var delete = new DeleteTodoHandler(db);
        var getTrashed = new GetTrashedTodosHandler(db);
        var handler = new PurgeExpiredTrashHandler(db);

        var oldId = await add.HandleAsync("Old trash");
        await delete.HandleAsync(oldId);
        var newId = await add.HandleAsync("New trash");
        await delete.HandleAsync(newId);

        using (var conn = db.CreateConnection())
        {
            var oldDate = DateTime.UtcNow.AddDays(-(PurgeExpiredTrashHandler.RetentionDays + 5)).ToString("O");
            await conn.ExecuteAsync(
                "UPDATE DeletedTodos SET DeletedAt = @DeletedAt WHERE Title = @Title",
                new { DeletedAt = oldDate, Title = "Old trash" });
        }

        var count = await handler.HandleAsync();

        Assert.Equal(1, count);
        var remaining = await getTrashed.HandleAsync();
        Assert.Single(remaining);
        Assert.Equal("New trash", remaining[0].Title);
    }

    [Fact]
    public async Task HandleAsync_DoesNotAffectActiveTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new PurgeExpiredTrashHandler(db);
        var getTodos = new TodoApp.Features.Todos.GetTodos.GetTodosHandler(db);

        await add.HandleAsync("Still active");

        await handler.HandleAsync();

        var todos = await getTodos.HandleAsync();
        Assert.Single(todos);
    }

    [Fact]
    public async Task HandleAsync_TodoExactlyAtRetentionBoundary_IsNotPurged()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var delete = new DeleteTodoHandler(db);
        var getTrashed = new GetTrashedTodosHandler(db);
        var handler = new PurgeExpiredTrashHandler(db);

        var id = await add.HandleAsync("Boundary trash");
        await delete.HandleAsync(id);

        using (var conn = db.CreateConnection())
        {
            var boundaryDate = DateTime.UtcNow.AddDays(-PurgeExpiredTrashHandler.RetentionDays + 1).ToString("O");
            await conn.ExecuteAsync("UPDATE DeletedTodos SET DeletedAt = @DeletedAt", new { DeletedAt = boundaryDate });
        }

        var count = await handler.HandleAsync();

        Assert.Equal(0, count);
        Assert.Single(await getTrashed.HandleAsync());
    }
}
