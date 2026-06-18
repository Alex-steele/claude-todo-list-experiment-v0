using TodoApp.Features.Lists;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.BulkOperations;
using TodoApp.Features.Todos.CompleteTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.Tags;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.BulkOperations;

public class BulkOperationsHandlerTests
{
    [Fact]
    public async Task CompleteAsync_MarksAllSelectedTodosComplete()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id1 = await addHandler.HandleAsync("Todo 1");
        var id2 = await addHandler.HandleAsync("Todo 2");
        var id3 = await addHandler.HandleAsync("Todo 3");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.CompleteAsync([id1, id2]);

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        Assert.True(todos.First(t => t.Id == id1).IsCompleted);
        Assert.True(todos.First(t => t.Id == id2).IsCompleted);
        Assert.False(todos.First(t => t.Id == id3).IsCompleted);
    }

    [Fact]
    public async Task DeleteAsync_RemovesAllSelectedTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id1 = await addHandler.HandleAsync("Todo 1");
        var id2 = await addHandler.HandleAsync("Todo 2");
        var id3 = await addHandler.HandleAsync("Todo 3");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.DeleteAsync([id1, id2]);

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        var remaining = Assert.Single(todos);
        Assert.Equal(id3, remaining.Id);
    }

    [Fact]
    public async Task CompleteAsync_WithEmptyList_DoesNothing()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Todo 1");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.CompleteAsync([]); // Should not throw

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        Assert.False(todos[0].IsCompleted);
    }

    [Fact]
    public async Task DeleteAsync_WithEmptyList_DoesNothing()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Todo 1");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.DeleteAsync([]); // Should not throw

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        Assert.Single(todos);
    }

    [Fact]
    public async Task CompleteAsync_AlreadyCompletedTodo_RemainsComplete()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var completeHandler = new CompleteTodoHandler(db);
        var id = await addHandler.HandleAsync("Todo 1");
        await completeHandler.HandleAsync(id); // Toggle to completed

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.CompleteAsync([id]); // Complete again via bulk

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        Assert.True(todos[0].IsCompleted);
    }

    [Fact]
    public async Task CompleteAsync_OnlyAffectsSpecifiedTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id1 = await addHandler.HandleAsync("Target");
        var id2 = await addHandler.HandleAsync("Bystander");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.CompleteAsync([id1]);

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        Assert.True(todos.First(t => t.Id == id1).IsCompleted);
        Assert.False(todos.First(t => t.Id == id2).IsCompleted);
    }

    [Fact]
    public async Task MoveAsync_MovesSelectedTodosToTargetList()
    {
        var db = await TestDatabase.CreateAsync();
        var createListHandler = new CreateListHandler(db);
        var addHandler = new AddTodoHandler(db);

        var workListId = await createListHandler.HandleAsync("Work");
        var id1 = await addHandler.HandleAsync("Task 1", listId: 1);
        var id2 = await addHandler.HandleAsync("Task 2", listId: 1);
        var id3 = await addHandler.HandleAsync("Task 3", listId: 1);

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.MoveAsync([id1, id2], workListId);

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        Assert.Equal(workListId, todos.First(t => t.Id == id1).ListId);
        Assert.Equal(workListId, todos.First(t => t.Id == id2).ListId);
        Assert.Equal(1, todos.First(t => t.Id == id3).ListId);
    }

    [Fact]
    public async Task MoveAsync_WithEmptyList_DoesNothing()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Task 1", listId: 1);

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.MoveAsync([], 99); // Should not throw

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        Assert.Equal(1, todos[0].ListId);
    }

    [Fact]
    public async Task MoveAsync_OnlyAffectsSpecifiedTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var createListHandler = new CreateListHandler(db);
        var addHandler = new AddTodoHandler(db);

        var workListId = await createListHandler.HandleAsync("Work");
        var id1 = await addHandler.HandleAsync("Target", listId: 1);
        var id2 = await addHandler.HandleAsync("Bystander", listId: 1);

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.MoveAsync([id1], workListId);

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        Assert.Equal(workListId, todos.First(t => t.Id == id1).ListId);
        Assert.Equal(1, todos.First(t => t.Id == id2).ListId);
    }

    [Fact]
    public async Task AddTagAsync_AppliesTagToAllSelectedTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id1 = await addHandler.HandleAsync("Todo 1");
        var id2 = await addHandler.HandleAsync("Todo 2");
        var id3 = await addHandler.HandleAsync("Todo 3");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.AddTagAsync([id1, id2], "work");

        var getTagsHandler = new GetTodoTagsHandler(db);
        var tags = await getTagsHandler.HandleAsync([id1, id2, id3]);
        Assert.Contains(tags[id1], t => t.Name == "work");
        Assert.Contains(tags[id2], t => t.Name == "work");
        Assert.DoesNotContain(tags.GetValueOrDefault(id3, []), t => t.Name == "work");
    }

    [Fact]
    public async Task AddTagAsync_NormalizesTagToLowercase()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id = await addHandler.HandleAsync("Todo");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.AddTagAsync([id], "  Work  ");

        var getTagsHandler = new GetTodoTagsHandler(db);
        var tags = await getTagsHandler.HandleAsync([id]);
        Assert.Contains(tags[id], t => t.Name == "work");
    }

    [Fact]
    public async Task AddTagAsync_DoesNotDuplicateExistingTag()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var addTagHandler = new AddTagHandler(db);
        var id = await addHandler.HandleAsync("Todo");
        await addTagHandler.HandleAsync(id, "work");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.AddTagAsync([id], "work");

        var getTagsHandler = new GetTodoTagsHandler(db);
        var tags = await getTagsHandler.HandleAsync([id]);
        Assert.Single(tags[id], t => t.Name == "work");
    }

    [Fact]
    public async Task AddTagAsync_WithEmptyList_DoesNothing()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id = await addHandler.HandleAsync("Todo");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.AddTagAsync([], "work");

        var getTagsHandler = new GetTodoTagsHandler(db);
        var tags = await getTagsHandler.HandleAsync([id]);
        Assert.Empty(tags.GetValueOrDefault(id, []));
    }

    [Fact]
    public async Task AddTagAsync_WithEmptyTagName_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id = await addHandler.HandleAsync("Todo");

        var bulkHandler = new BulkOperationsHandler(db);
        await Assert.ThrowsAsync<ArgumentException>(() => bulkHandler.AddTagAsync([id], "   "));
    }
}
