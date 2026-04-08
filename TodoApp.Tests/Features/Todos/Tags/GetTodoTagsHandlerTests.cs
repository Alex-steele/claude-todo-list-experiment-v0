using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.Tags;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Tags;

public class GetTodoTagsHandlerTests
{
    [Fact]
    public async Task GetTodoTags_EmptyList_ReturnsEmptyDictionary()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new GetTodoTagsHandler(db);

        var result = await handler.HandleAsync([]);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTodoTags_TodoWithNoTags_NotInResult()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var handler = new GetTodoTagsHandler(db);

        var todoId = await addTodo.HandleAsync("Buy milk");

        var result = await handler.HandleAsync([todoId]);

        Assert.False(result.ContainsKey(todoId));
    }

    [Fact]
    public async Task GetTodoTags_GroupsTagsByTodoId()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addTag = new AddTagHandler(db);
        var handler = new GetTodoTagsHandler(db);

        var todo1 = await addTodo.HandleAsync("Todo 1");
        var todo2 = await addTodo.HandleAsync("Todo 2");

        await addTag.HandleAsync(todo1, "work");
        await addTag.HandleAsync(todo1, "urgent");
        await addTag.HandleAsync(todo2, "personal");

        var result = await handler.HandleAsync([todo1, todo2]);

        Assert.Equal(2, result[todo1].Count);
        Assert.Single(result[todo2]);
        Assert.Equal("personal", result[todo2][0].Name);
    }

    [Fact]
    public async Task GetTodoTags_ReturnsTagsInAlphabeticalOrder()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addTag = new AddTagHandler(db);
        var handler = new GetTodoTagsHandler(db);

        var todoId = await addTodo.HandleAsync("Todo");
        await addTag.HandleAsync(todoId, "zebra");
        await addTag.HandleAsync(todoId, "apple");
        await addTag.HandleAsync(todoId, "mango");

        var result = await handler.HandleAsync([todoId]);
        var names = result[todoId].Select(t => t.Name).ToList();

        Assert.Equal(["apple", "mango", "zebra"], names);
    }
}
