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

    // RenameListHandler tests

    [Fact]
    public async Task RenameList_UpdatesName()
    {
        var db = await TestDatabase.CreateAsync();
        var createHandler = new CreateListHandler(db);
        var renameHandler = new RenameListHandler(db);
        var getHandler = new GetListsHandler(db);

        var id = await createHandler.HandleAsync("Work");
        await renameHandler.HandleAsync(id, "Projects");

        var lists = await getHandler.HandleAsync();
        Assert.Contains(lists, l => l.Name == "Projects");
        Assert.DoesNotContain(lists, l => l.Name == "Work");
    }

    [Fact]
    public async Task RenameList_TrimsWhitespace()
    {
        var db = await TestDatabase.CreateAsync();
        var createHandler = new CreateListHandler(db);
        var renameHandler = new RenameListHandler(db);
        var getHandler = new GetListsHandler(db);

        var id = await createHandler.HandleAsync("Work");
        await renameHandler.HandleAsync(id, "  Projects  ");

        var lists = await getHandler.HandleAsync();
        Assert.Contains(lists, l => l.Name == "Projects");
    }

    [Fact]
    public async Task RenameList_EmptyName_Throws()
    {
        var db = await TestDatabase.CreateAsync();
        var createHandler = new CreateListHandler(db);
        var renameHandler = new RenameListHandler(db);

        var id = await createHandler.HandleAsync("Work");

        await Assert.ThrowsAsync<ArgumentException>(() => renameHandler.HandleAsync(id, "  "));
    }

    [Fact]
    public async Task RenameList_DefaultList_CanBeRenamed()
    {
        var db = await TestDatabase.CreateAsync();
        var renameHandler = new RenameListHandler(db);
        var getHandler = new GetListsHandler(db);

        await renameHandler.HandleAsync(1, "Home");

        var lists = await getHandler.HandleAsync();
        Assert.Contains(lists, l => l.Id == 1 && l.Name == "Home");
    }

    [Fact]
    public async Task RenameList_NotFound_Throws()
    {
        var db = await TestDatabase.CreateAsync();
        var renameHandler = new RenameListHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() => renameHandler.HandleAsync(999, "Ghost"));
    }

    // ReorderListsHandler tests

    [Fact]
    public async Task ReorderLists_PersistsNewOrder()
    {
        var db = await TestDatabase.CreateAsync();
        var create = new CreateListHandler(db);
        var reorder = new ReorderListsHandler(db);
        var get = new GetListsHandler(db);

        var id2 = await create.HandleAsync("Work");
        var id3 = await create.HandleAsync("Shopping");

        // Reorder: Shopping first, then Work, then Personal (id=1)
        await reorder.HandleAsync([id3, id2, 1]);

        var lists = await get.HandleAsync();
        Assert.Equal(id3, lists[0].Id);
        Assert.Equal(id2, lists[1].Id);
        Assert.Equal(1,   lists[2].Id);
    }

    [Fact]
    public async Task ReorderLists_EmptyList_DoesNotThrow()
    {
        var db = await TestDatabase.CreateAsync();
        var reorder = new ReorderListsHandler(db);

        var ex = await Record.ExceptionAsync(() => reorder.HandleAsync([]));
        Assert.Null(ex);
    }

    [Fact]
    public async Task ReorderLists_SingleList_RemainsFirst()
    {
        var db = await TestDatabase.CreateAsync();
        var reorder = new ReorderListsHandler(db);
        var get = new GetListsHandler(db);

        await reorder.HandleAsync([1]);

        var lists = await get.HandleAsync();
        Assert.Single(lists);
        Assert.Equal(1, lists[0].Id);
    }

    [Fact]
    public async Task GetLists_AfterReorder_ReturnsListsInNewOrder()
    {
        var db = await TestDatabase.CreateAsync();
        var create = new CreateListHandler(db);
        var reorder = new ReorderListsHandler(db);
        var get = new GetListsHandler(db);

        var workId = await create.HandleAsync("Work");
        var shoppingId = await create.HandleAsync("Shopping");

        // Initial order: Personal(1), Work, Shopping
        var before = await get.HandleAsync();
        Assert.Equal(1, before[0].Id);
        Assert.Equal(workId, before[1].Id);
        Assert.Equal(shoppingId, before[2].Id);

        // Reverse order
        await reorder.HandleAsync([shoppingId, workId, 1]);

        var after = await get.HandleAsync();
        Assert.Equal(shoppingId, after[0].Id);
        Assert.Equal(workId, after[1].Id);
        Assert.Equal(1, after[2].Id);
    }

    [Fact]
    public async Task ReorderLists_PartialReorder_UpdatesSpecifiedLists()
    {
        var db = await TestDatabase.CreateAsync();
        var create = new CreateListHandler(db);
        var reorder = new ReorderListsHandler(db);
        var get = new GetListsHandler(db);

        var id2 = await create.HandleAsync("Work");

        // Swap: Work first, Personal second
        await reorder.HandleAsync([id2, 1]);

        var lists = await get.HandleAsync();
        Assert.Equal(id2, lists[0].Id);
        Assert.Equal(1, lists[1].Id);
    }
}
