using TodoApp.Features.Lists;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Lists;

public class ListHandlerTests
{
    [Fact]
    public async Task GetLists_ReturnsDefaultList()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new GetListsHandler(db);

        var lists = await handler.HandleAsync();

        Assert.Single(lists);
        Assert.Equal(1, lists[0].Id);
        Assert.Equal("Personal", lists[0].Name);
    }

    [Fact]
    public async Task CreateList_AddsNewList()
    {
        var db = await TestDatabase.CreateAsync();
        var createHandler = new CreateListHandler(db);
        var getHandler = new GetListsHandler(db);

        await createHandler.HandleAsync("Work");

        var lists = await getHandler.HandleAsync();
        Assert.Equal(2, lists.Count);
        Assert.Contains(lists, l => l.Name == "Work");
    }

    [Fact]
    public async Task CreateList_ReturnsNewId()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new CreateListHandler(db);

        var id = await handler.HandleAsync("Shopping");

        Assert.True(id > 1);
    }

    [Fact]
    public async Task CreateList_EmptyName_Throws()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new CreateListHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync("  "));
    }

    [Fact]
    public async Task DeleteList_RemovesList()
    {
        var db = await TestDatabase.CreateAsync();
        var createHandler = new CreateListHandler(db);
        var deleteHandler = new DeleteListHandler(db);
        var getHandler = new GetListsHandler(db);

        var id = await createHandler.HandleAsync("Work");
        await deleteHandler.HandleAsync(id);

        var lists = await getHandler.HandleAsync();
        Assert.DoesNotContain(lists, l => l.Name == "Work");
    }

    [Fact]
    public async Task DeleteList_MovesTodosToDefault()
    {
        var db = await TestDatabase.CreateAsync();
        var createListHandler = new CreateListHandler(db);
        var deleteHandler = new DeleteListHandler(db);
        var addTodoHandler = new AddTodoHandler(db);
        var getTodosHandler = new GetTodosHandler(db);

        var workId = await createListHandler.HandleAsync("Work");
        await addTodoHandler.HandleAsync("Work task", listId: workId);

        await deleteHandler.HandleAsync(workId);

        var todos = await getTodosHandler.HandleAsync();
        Assert.Single(todos);
        Assert.Equal(DeleteListHandler.DefaultListId, todos[0].ListId);
    }

    [Fact]
    public async Task DeleteList_DefaultList_Throws()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new DeleteListHandler(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(DeleteListHandler.DefaultListId));
    }

    [Fact]
    public async Task AddTodo_WithListId_StoresCorrectList()
    {
        var db = await TestDatabase.CreateAsync();
        var createListHandler = new CreateListHandler(db);
        var addTodoHandler = new AddTodoHandler(db);
        var getTodosHandler = new GetTodosHandler(db);

        var workId = await createListHandler.HandleAsync("Work");
        await addTodoHandler.HandleAsync("Work task", listId: workId);

        var todos = await getTodosHandler.HandleAsync();
        Assert.Single(todos);
        Assert.Equal(workId, todos[0].ListId);
    }

    [Fact]
    public async Task AddTodo_DefaultListId_WhenNotSpecified()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        await addHandler.HandleAsync("Default task");

        var todos = await getHandler.HandleAsync();
        Assert.Equal(1, todos[0].ListId);
    }
}
