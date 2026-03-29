using TodoApp.Features.Todos;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.CompleteTodo;
using TodoApp.Features.Todos.DeleteTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.UndoRedo;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.UndoRedo;

public class RestoreTodosHandlerTests
{
    [Fact]
    public async Task RestoreDeletedTodo_ReappearsInList()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var deleteHandler = new DeleteTodoHandler(db);
        var getTodosHandler = new GetTodosHandler(db);
        var restoreHandler = new RestoreTodosHandler(db);

        var id = await addHandler.HandleAsync("Buy milk");
        var todosBeforeDelete = await getTodosHandler.HandleAsync();
        var todoToRestore = todosBeforeDelete.Single(t => t.Id == id);

        await deleteHandler.HandleAsync(id);
        Assert.Empty(await getTodosHandler.HandleAsync());

        await restoreHandler.HandleAsync([todoToRestore]);

        var todosAfterRestore = await getTodosHandler.HandleAsync();
        Assert.Single(todosAfterRestore);
        Assert.Equal("Buy milk", todosAfterRestore[0].Title);
    }

    [Fact]
    public async Task RestorePreservesOriginalTodoData()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var deleteHandler = new DeleteTodoHandler(db);
        var getTodosHandler = new GetTodosHandler(db);
        var restoreHandler = new RestoreTodosHandler(db);

        var dueDate = DateTime.Today.AddDays(3);
        var id = await addHandler.HandleAsync("Important task", TodoPriority.High, dueDate);
        var original = (await getTodosHandler.HandleAsync()).Single();

        await deleteHandler.HandleAsync(id);
        await restoreHandler.HandleAsync([original]);

        var restored = (await getTodosHandler.HandleAsync()).Single();
        Assert.Equal(original.Id, restored.Id);
        Assert.Equal("Important task", restored.Title);
        Assert.Equal(TodoPriority.High, restored.Priority);
        Assert.Equal(dueDate.Date, restored.DueDate?.Date);
        Assert.False(restored.IsCompleted);
    }

    [Fact]
    public async Task RestoreMultipleDeletedTodos_AllReappear()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var deleteHandler = new DeleteTodoHandler(db);
        var getTodosHandler = new GetTodosHandler(db);
        var restoreHandler = new RestoreTodosHandler(db);

        await addHandler.HandleAsync("Todo A");
        await addHandler.HandleAsync("Todo B");
        await addHandler.HandleAsync("Todo C");

        var allTodos = await getTodosHandler.HandleAsync();
        foreach (var t in allTodos)
            await deleteHandler.HandleAsync(t.Id);

        Assert.Empty(await getTodosHandler.HandleAsync());

        await restoreHandler.HandleAsync(allTodos);

        var restored = await getTodosHandler.HandleAsync();
        Assert.Equal(3, restored.Count);
        Assert.Contains(restored, t => t.Title == "Todo A");
        Assert.Contains(restored, t => t.Title == "Todo B");
        Assert.Contains(restored, t => t.Title == "Todo C");
    }

    [Fact]
    public async Task RestoreEmptyList_DoesNothing()
    {
        var db = await TestDatabase.CreateAsync();
        var getTodosHandler = new GetTodosHandler(db);
        var restoreHandler = new RestoreTodosHandler(db);

        await restoreHandler.HandleAsync([]);

        Assert.Empty(await getTodosHandler.HandleAsync());
    }

    [Fact]
    public async Task RestoreTodoThatAlreadyExists_IsIgnored()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getTodosHandler = new GetTodosHandler(db);
        var restoreHandler = new RestoreTodosHandler(db);

        var id = await addHandler.HandleAsync("Existing todo");
        var todo = (await getTodosHandler.HandleAsync()).Single();

        // Restore when the todo is still present — should not create a duplicate
        await restoreHandler.HandleAsync([todo]);

        var todos = await getTodosHandler.HandleAsync();
        Assert.Single(todos);
        Assert.Equal(id, todos[0].Id);
    }
}
