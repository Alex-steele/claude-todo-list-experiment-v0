using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.BlockTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.BlockTodo;

public class BlockTodoHandlerTests
{
    [Fact]
    public async Task BlockTodo_UnblockedTodo_BecomesBlocked()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var block = new BlockTodoHandler(db);
        var get = new GetTodosHandler(db);

        var id = await add.HandleAsync("Review PR");
        await block.HandleAsync(id);

        var todos = await get.HandleAsync();
        Assert.True(todos.Single(t => t.Id == id).IsBlocked);
    }

    [Fact]
    public async Task BlockTodo_BlockedTodo_BecomesUnblocked()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var block = new BlockTodoHandler(db);
        var get = new GetTodosHandler(db);

        var id = await add.HandleAsync("Review PR");
        await block.HandleAsync(id); // block
        await block.HandleAsync(id); // unblock

        var todos = await get.HandleAsync();
        Assert.False(todos.Single(t => t.Id == id).IsBlocked);
    }

    [Fact]
    public async Task NewTodo_DefaultsToUnblocked()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var get = new GetTodosHandler(db);

        var id = await add.HandleAsync("Fresh task");

        var todos = await get.HandleAsync();
        Assert.False(todos.Single(t => t.Id == id).IsBlocked);
    }

    [Fact]
    public async Task BlockTodo_OnlyAffectsTargetTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var block = new BlockTodoHandler(db);
        var get = new GetTodosHandler(db);

        var id1 = await add.HandleAsync("Task A");
        var id2 = await add.HandleAsync("Task B");
        await block.HandleAsync(id1);

        var todos = await get.HandleAsync();
        Assert.True(todos.Single(t => t.Id == id1).IsBlocked);
        Assert.False(todos.Single(t => t.Id == id2).IsBlocked);
    }

    [Fact]
    public async Task BlockTodo_ToggleThreeTimes_EndsBlocked()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var block = new BlockTodoHandler(db);
        var get = new GetTodosHandler(db);

        var id = await add.HandleAsync("Waiting on design");
        await block.HandleAsync(id); // blocked
        await block.HandleAsync(id); // unblocked
        await block.HandleAsync(id); // blocked again

        var todos = await get.HandleAsync();
        Assert.True(todos.Single(t => t.Id == id).IsBlocked);
    }
}
