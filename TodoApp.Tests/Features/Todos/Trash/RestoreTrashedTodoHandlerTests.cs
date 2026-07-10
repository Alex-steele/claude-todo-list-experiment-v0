using TodoApp.Features.Todos;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.DeleteTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.Trash;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Trash;

public class RestoreTrashedTodoHandlerTests
{
    [Fact]
    public async Task HandleAsync_RestoresTodoToActiveList()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var delete = new DeleteTodoHandler(db);
        var getTrashed = new GetTrashedTodosHandler(db);
        var restore = new RestoreTrashedTodoHandler(db);
        var getTodos = new GetTodosHandler(db);

        var id = await add.HandleAsync("Buy milk", TodoPriority.High);
        await delete.HandleAsync(id);
        var trashId = (await getTrashed.HandleAsync()).Single().TrashId;

        await restore.HandleAsync(trashId);

        var todos = await getTodos.HandleAsync();
        var restored = Assert.Single(todos);
        Assert.Equal("Buy milk", restored.Title);
        Assert.Equal(TodoPriority.High, restored.Priority);
    }

    [Fact]
    public async Task HandleAsync_RemovesEntryFromTrash()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var delete = new DeleteTodoHandler(db);
        var getTrashed = new GetTrashedTodosHandler(db);
        var restore = new RestoreTrashedTodoHandler(db);

        var id = await add.HandleAsync("Buy milk");
        await delete.HandleAsync(id);
        var trashId = (await getTrashed.HandleAsync()).Single().TrashId;

        await restore.HandleAsync(trashId);

        Assert.Empty(await getTrashed.HandleAsync());
    }

    [Fact]
    public async Task HandleAsync_NonExistentTrashId_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        var restore = new RestoreTrashedTodoHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() => restore.HandleAsync(999));
    }

    [Fact]
    public async Task HandleAsync_PreservesCompletedStateAndDueDate()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var delete = new DeleteTodoHandler(db);
        var getTrashed = new GetTrashedTodosHandler(db);
        var restore = new RestoreTrashedTodoHandler(db);
        var getTodos = new GetTodosHandler(db);

        var dueDate = DateTime.Today.AddDays(1);
        var id = await add.HandleAsync("Task with date", TodoPriority.Low, dueDate);
        await delete.HandleAsync(id);
        var trashId = (await getTrashed.HandleAsync()).Single().TrashId;

        await restore.HandleAsync(trashId);

        var restored = (await getTodos.HandleAsync()).Single();
        Assert.Equal(dueDate.Date, restored.DueDate?.Date);
        Assert.False(restored.IsCompleted);
    }
}
