using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.Links;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Links;

public class SetTodoUrlHandlerTests
{
    [Fact]
    public async Task SetUrl_SavesUrlToTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new SetTodoUrlHandler(db);
        var get = new GetTodosHandler(db);

        var id = await add.HandleAsync("Review PR");
        await handler.HandleAsync(id, "https://github.com/org/repo/pull/42");

        var todos = await get.HandleAsync();
        Assert.Equal("https://github.com/org/repo/pull/42", todos.Single().Url);
    }

    [Fact]
    public async Task SetUrl_TrimsWhitespace()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new SetTodoUrlHandler(db);
        var get = new GetTodosHandler(db);

        var id = await add.HandleAsync("Review PR");
        await handler.HandleAsync(id, "  https://example.com  ");

        var todos = await get.HandleAsync();
        Assert.Equal("https://example.com", todos.Single().Url);
    }

    [Fact]
    public async Task SetUrl_EmptyString_StoresNull()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new SetTodoUrlHandler(db);
        var get = new GetTodosHandler(db);

        var id = await add.HandleAsync("Review PR");
        await handler.HandleAsync(id, "https://example.com");
        await handler.HandleAsync(id, "   ");

        var todos = await get.HandleAsync();
        Assert.Null(todos.Single().Url);
    }

    [Fact]
    public async Task SetUrl_Null_ClearsUrl()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new SetTodoUrlHandler(db);
        var get = new GetTodosHandler(db);

        var id = await add.HandleAsync("Review PR");
        await handler.HandleAsync(id, "https://example.com");
        await handler.HandleAsync(id, null);

        var todos = await get.HandleAsync();
        Assert.Null(todos.Single().Url);
    }

    [Fact]
    public async Task SetUrl_CanUpdateExistingUrl()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new SetTodoUrlHandler(db);
        var get = new GetTodosHandler(db);

        var id = await add.HandleAsync("Review PR");
        await handler.HandleAsync(id, "https://example.com/old");
        await handler.HandleAsync(id, "https://example.com/new");

        var todos = await get.HandleAsync();
        Assert.Equal("https://example.com/new", todos.Single().Url);
    }

    [Fact]
    public async Task SetUrl_OnlyAffectsTargetTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new SetTodoUrlHandler(db);
        var get = new GetTodosHandler(db);

        var id1 = await add.HandleAsync("Todo A");
        var id2 = await add.HandleAsync("Todo B");
        await handler.HandleAsync(id1, "https://example.com");

        var todos = await get.HandleAsync();
        Assert.Equal("https://example.com", todos.Single(t => t.Id == id1).Url);
        Assert.Null(todos.Single(t => t.Id == id2).Url);
    }

    [Fact]
    public async Task NewTodo_DefaultsToNullUrl()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var get = new GetTodosHandler(db);

        await add.HandleAsync("Review PR");

        var todos = await get.HandleAsync();
        Assert.Null(todos.Single().Url);
    }
}
