using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.Subtasks;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Subtasks;

public class ReorderSubtasksHandlerTests
{
    [Fact]
    public async Task Reorder_ChangesOrder()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addSubtask = new AddSubtaskHandler(db);
        var reorderHandler = new ReorderSubtasksHandler(db);
        var getHandler = new GetSubtasksHandler(db);

        var todoId = await addTodo.HandleAsync("Main task");
        var id1 = await addSubtask.HandleAsync(todoId, "First");
        var id2 = await addSubtask.HandleAsync(todoId, "Second");
        var id3 = await addSubtask.HandleAsync(todoId, "Third");

        // Reverse the order
        await reorderHandler.HandleAsync([id3, id2, id1]);

        var subtasks = await getHandler.HandleAsync([todoId]);
        Assert.Equal(id3, subtasks[todoId][0].Id);
        Assert.Equal(id2, subtasks[todoId][1].Id);
        Assert.Equal(id1, subtasks[todoId][2].Id);
    }

    [Fact]
    public async Task Reorder_EmptyList_DoesNothing()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new ReorderSubtasksHandler(db);

        // Should not throw
        await handler.HandleAsync([]);
    }

    [Fact]
    public async Task Reorder_SingleItem_Succeeds()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addSubtask = new AddSubtaskHandler(db);
        var reorderHandler = new ReorderSubtasksHandler(db);

        var todoId = await addTodo.HandleAsync("Main task");
        var id = await addSubtask.HandleAsync(todoId, "Only subtask");

        await reorderHandler.HandleAsync([id]);
    }

    [Fact]
    public async Task Reorder_PreservesExistingData()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addSubtask = new AddSubtaskHandler(db);
        var reorderHandler = new ReorderSubtasksHandler(db);
        var getHandler = new GetSubtasksHandler(db);

        var todoId = await addTodo.HandleAsync("Main task");
        var id1 = await addSubtask.HandleAsync(todoId, "Alpha");
        var id2 = await addSubtask.HandleAsync(todoId, "Beta");

        await reorderHandler.HandleAsync([id2, id1]);

        var subtasks = await getHandler.HandleAsync([todoId]);
        Assert.Equal("Beta", subtasks[todoId][0].Title);
        Assert.Equal("Alpha", subtasks[todoId][1].Title);
    }

    [Fact]
    public async Task Reorder_DoesNotAffectOtherTodosSubtasks()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addSubtask = new AddSubtaskHandler(db);
        var reorderHandler = new ReorderSubtasksHandler(db);
        var getHandler = new GetSubtasksHandler(db);

        var todo1 = await addTodo.HandleAsync("Todo 1");
        var todo2 = await addTodo.HandleAsync("Todo 2");
        var t1a = await addSubtask.HandleAsync(todo1, "1A");
        var t1b = await addSubtask.HandleAsync(todo1, "1B");
        var t2a = await addSubtask.HandleAsync(todo2, "2A");
        var t2b = await addSubtask.HandleAsync(todo2, "2B");

        await reorderHandler.HandleAsync([t1b, t1a]);

        var subtasks = await getHandler.HandleAsync([todo1, todo2]);
        Assert.Equal(["1B", "1A"], subtasks[todo1].Select(s => s.Title));
        Assert.Equal(["2A", "2B"], subtasks[todo2].Select(s => s.Title));
    }
}
