using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.Tags;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Tags;

public class GetAllTagNamesHandlerTests
{
    [Fact]
    public async Task GetAllTagNames_NoTags_ReturnsEmpty()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new GetAllTagNamesHandler(db);

        var names = await handler.HandleAsync();

        Assert.Empty(names);
    }

    [Fact]
    public async Task GetAllTagNames_WithTags_ReturnsAllDistinctNames()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addTag = new AddTagHandler(db);
        var handler = new GetAllTagNamesHandler(db);

        var id1 = await addTodo.HandleAsync("Task one");
        var id2 = await addTodo.HandleAsync("Task two");
        await addTag.HandleAsync(id1, "work");
        await addTag.HandleAsync(id1, "urgent");
        await addTag.HandleAsync(id2, "personal");

        var names = await handler.HandleAsync();

        Assert.Equal(3, names.Count);
        Assert.Contains("work", names);
        Assert.Contains("urgent", names);
        Assert.Contains("personal", names);
    }

    [Fact]
    public async Task GetAllTagNames_DuplicatesAcrossTodos_ReturnedOnce()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addTag = new AddTagHandler(db);
        var handler = new GetAllTagNamesHandler(db);

        var id1 = await addTodo.HandleAsync("Task one");
        var id2 = await addTodo.HandleAsync("Task two");
        await addTag.HandleAsync(id1, "work");
        await addTag.HandleAsync(id2, "work");

        var names = await handler.HandleAsync();

        Assert.Single(names);
        Assert.Equal("work", names[0]);
    }

    [Fact]
    public async Task GetAllTagNames_ReturnsNamesInAlphabeticalOrder()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addTag = new AddTagHandler(db);
        var handler = new GetAllTagNamesHandler(db);

        var id = await addTodo.HandleAsync("Task");
        await addTag.HandleAsync(id, "zebra");
        await addTag.HandleAsync(id, "apple");
        await addTag.HandleAsync(id, "mango");

        var names = await handler.HandleAsync();

        Assert.Equal(["apple", "mango", "zebra"], names);
    }
}
