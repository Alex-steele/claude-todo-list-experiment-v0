using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.Tags;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Tags;

public class RenameTagHandlerTests
{
    [Fact]
    public async Task RenameTag_UpdatesNameOnAllTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addTag = new AddTagHandler(db);
        var handler = new RenameTagHandler(db);
        var getTags = new GetTodoTagsHandler(db);

        var id1 = await addTodo.HandleAsync("Todo 1");
        var id2 = await addTodo.HandleAsync("Todo 2");
        await addTag.HandleAsync(id1, "work");
        await addTag.HandleAsync(id2, "work");

        await handler.HandleAsync("work", "job");

        var tags = await getTags.HandleAsync([id1, id2]);
        Assert.Equal("job", tags[id1][0].Name);
        Assert.Equal("job", tags[id2][0].Name);
    }

    [Fact]
    public async Task RenameTag_EmptyNewName_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addTag = new AddTagHandler(db);
        var handler = new RenameTagHandler(db);

        var id = await addTodo.HandleAsync("Todo");
        await addTag.HandleAsync(id, "work");

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync("work", ""));
        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync("work", "   "));
    }

    [Fact]
    public async Task RenameTag_SameNameAsOld_DoesNothing()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addTag = new AddTagHandler(db);
        var handler = new RenameTagHandler(db);
        var getTags = new GetTodoTagsHandler(db);

        var id = await addTodo.HandleAsync("Todo");
        await addTag.HandleAsync(id, "work");

        await handler.HandleAsync("work", "work");

        var tags = await getTags.HandleAsync([id]);
        Assert.Single(tags[id]);
        Assert.Equal("work", tags[id][0].Name);
    }

    [Fact]
    public async Task RenameTag_NormalizesNewNameToLowercase()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addTag = new AddTagHandler(db);
        var handler = new RenameTagHandler(db);
        var getTags = new GetTodoTagsHandler(db);

        var id = await addTodo.HandleAsync("Todo");
        await addTag.HandleAsync(id, "work");

        await handler.HandleAsync("work", "URGENT");

        var tags = await getTags.HandleAsync([id]);
        Assert.Equal("urgent", tags[id][0].Name);
    }

    [Fact]
    public async Task RenameTag_WhenTodoAlreadyHasNewName_DoesNotCreateDuplicate()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addTag = new AddTagHandler(db);
        var handler = new RenameTagHandler(db);
        var getTags = new GetTodoTagsHandler(db);

        var id = await addTodo.HandleAsync("Todo");
        await addTag.HandleAsync(id, "work");
        await addTag.HandleAsync(id, "job");

        // Renaming "work" to "job" — todo already has "job", so "work" should be removed, not duplicated
        await handler.HandleAsync("work", "job");

        var tags = await getTags.HandleAsync([id]);
        Assert.Single(tags[id]);
        Assert.Equal("job", tags[id][0].Name);
    }

    [Fact]
    public async Task RenameTag_OnlyRenamesMatchingTag_LeavesOthersIntact()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addTag = new AddTagHandler(db);
        var handler = new RenameTagHandler(db);
        var getTags = new GetTodoTagsHandler(db);

        var id = await addTodo.HandleAsync("Todo");
        await addTag.HandleAsync(id, "work");
        await addTag.HandleAsync(id, "urgent");

        await handler.HandleAsync("work", "project");

        var tags = await getTags.HandleAsync([id]);
        Assert.Equal(2, tags[id].Count);
        Assert.Contains(tags[id], t => t.Name == "project");
        Assert.Contains(tags[id], t => t.Name == "urgent");
    }

    [Fact]
    public async Task RenameTag_NonExistentTag_DoesNothing()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new RenameTagHandler(db);

        // Should not throw for a tag that does not exist
        await handler.HandleAsync("ghost", "phantom");
    }
}
