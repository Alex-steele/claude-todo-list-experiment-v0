using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.Tags;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Tags;

public class RemoveTagHandlerTests
{
    [Fact]
    public async Task RemoveTag_ExistingTag_IsDeleted()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addTag = new AddTagHandler(db);
        var removeHandler = new RemoveTagHandler(db);
        var getHandler = new GetTodoTagsHandler(db);

        var todoId = await addTodo.HandleAsync("Buy milk");
        await addTag.HandleAsync(todoId, "shopping");

        var before = await getHandler.HandleAsync([todoId]);
        var tagId = before[todoId][0].Id;

        await removeHandler.HandleAsync(tagId);

        var after = await getHandler.HandleAsync([todoId]);
        Assert.False(after.ContainsKey(todoId));
    }

    [Fact]
    public async Task RemoveTag_NonExistentId_DoesNotThrow()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new RemoveTagHandler(db);

        // Should complete without exception
        await handler.HandleAsync(99999);
    }

    [Fact]
    public async Task RemoveTag_OnlyRemovesTargetTag()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addTag = new AddTagHandler(db);
        var removeHandler = new RemoveTagHandler(db);
        var getHandler = new GetTodoTagsHandler(db);

        var todoId = await addTodo.HandleAsync("Buy milk");
        await addTag.HandleAsync(todoId, "shopping");
        await addTag.HandleAsync(todoId, "urgent");

        var before = await getHandler.HandleAsync([todoId]);
        var shoppingTag = before[todoId].First(t => t.Name == "shopping");

        await removeHandler.HandleAsync(shoppingTag.Id);

        var after = await getHandler.HandleAsync([todoId]);
        Assert.Single(after[todoId]);
        Assert.Equal("urgent", after[todoId][0].Name);
    }
}
