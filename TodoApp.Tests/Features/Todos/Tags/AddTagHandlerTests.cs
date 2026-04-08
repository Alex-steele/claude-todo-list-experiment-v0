using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.Tags;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Tags;

public class AddTagHandlerTests
{
    [Fact]
    public async Task AddTag_ValidName_IsStoredForTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var handler = new AddTagHandler(db);
        var getHandler = new GetTodoTagsHandler(db);

        var todoId = await addTodo.HandleAsync("Buy milk");
        await handler.HandleAsync(todoId, "shopping");

        var tags = await getHandler.HandleAsync([todoId]);
        Assert.True(tags.ContainsKey(todoId));
        Assert.Single(tags[todoId]);
        Assert.Equal("shopping", tags[todoId][0].Name);
    }

    [Fact]
    public async Task AddTag_EmptyName_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var handler = new AddTagHandler(db);

        var todoId = await addTodo.HandleAsync("Buy milk");

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(todoId, ""));
        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(todoId, "   "));
    }

    [Fact]
    public async Task AddTag_NormalizesToLowercase()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var handler = new AddTagHandler(db);
        var getHandler = new GetTodoTagsHandler(db);

        var todoId = await addTodo.HandleAsync("Buy milk");
        await handler.HandleAsync(todoId, "URGENT");

        var tags = await getHandler.HandleAsync([todoId]);
        Assert.Equal("urgent", tags[todoId][0].Name);
    }

    [Fact]
    public async Task AddTag_DuplicateTag_IsNotStoredTwice()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var handler = new AddTagHandler(db);
        var getHandler = new GetTodoTagsHandler(db);

        var todoId = await addTodo.HandleAsync("Buy milk");
        await handler.HandleAsync(todoId, "shopping");
        await handler.HandleAsync(todoId, "shopping");

        var tags = await getHandler.HandleAsync([todoId]);
        Assert.Single(tags[todoId]);
    }

    [Fact]
    public async Task AddTag_MultipleTags_AllStored()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var handler = new AddTagHandler(db);
        var getHandler = new GetTodoTagsHandler(db);

        var todoId = await addTodo.HandleAsync("Buy milk");
        await handler.HandleAsync(todoId, "shopping");
        await handler.HandleAsync(todoId, "urgent");

        var tags = await getHandler.HandleAsync([todoId]);
        Assert.Equal(2, tags[todoId].Count);
    }
}
