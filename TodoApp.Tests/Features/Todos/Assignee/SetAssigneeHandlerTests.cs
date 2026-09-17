using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.Assignee;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Assignee;

public class SetAssigneeHandlerTests
{
    [Fact]
    public async Task SetAssignee_SavesAssigneeToTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new SetAssigneeHandler(db);
        var get = new GetTodosHandler(db);

        var id = await add.HandleAsync("Review PR");
        await handler.HandleAsync(id, "Alice");

        var todos = await get.HandleAsync();
        Assert.Equal("Alice", todos.Single().Assignee);
    }

    [Fact]
    public async Task SetAssignee_TrimsWhitespace()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new SetAssigneeHandler(db);
        var get = new GetTodosHandler(db);

        var id = await add.HandleAsync("Review PR");
        await handler.HandleAsync(id, "  Bob  ");

        var todos = await get.HandleAsync();
        Assert.Equal("Bob", todos.Single().Assignee);
    }

    [Fact]
    public async Task SetAssignee_EmptyString_StoresNull()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new SetAssigneeHandler(db);
        var get = new GetTodosHandler(db);

        var id = await add.HandleAsync("Review PR");
        await handler.HandleAsync(id, "Alice");
        await handler.HandleAsync(id, "   ");

        var todos = await get.HandleAsync();
        Assert.Null(todos.Single().Assignee);
    }

    [Fact]
    public async Task SetAssignee_Null_ClearsAssignee()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new SetAssigneeHandler(db);
        var get = new GetTodosHandler(db);

        var id = await add.HandleAsync("Review PR");
        await handler.HandleAsync(id, "Alice");
        await handler.HandleAsync(id, null);

        var todos = await get.HandleAsync();
        Assert.Null(todos.Single().Assignee);
    }

    [Fact]
    public async Task SetAssignee_CanUpdateExistingAssignee()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new SetAssigneeHandler(db);
        var get = new GetTodosHandler(db);

        var id = await add.HandleAsync("Review PR");
        await handler.HandleAsync(id, "Alice");
        await handler.HandleAsync(id, "Bob");

        var todos = await get.HandleAsync();
        Assert.Equal("Bob", todos.Single().Assignee);
    }

    [Fact]
    public async Task SetAssignee_OnlyAffectsTargetTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new SetAssigneeHandler(db);
        var get = new GetTodosHandler(db);

        var id1 = await add.HandleAsync("Todo A");
        var id2 = await add.HandleAsync("Todo B");
        await handler.HandleAsync(id1, "Alice");

        var todos = await get.HandleAsync();
        Assert.Equal("Alice", todos.Single(t => t.Id == id1).Assignee);
        Assert.Null(todos.Single(t => t.Id == id2).Assignee);
    }

    [Fact]
    public async Task NewTodo_DefaultsToNullAssignee()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var get = new GetTodosHandler(db);

        await add.HandleAsync("Review PR");

        var todos = await get.HandleAsync();
        Assert.Null(todos.Single().Assignee);
    }
}
