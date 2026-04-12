using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.Subtasks;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Subtasks;

public class CompleteSubtaskHandlerTests
{
    [Fact]
    public async Task CompleteSubtask_TogglesCompletionState()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addSubtask = new AddSubtaskHandler(db);
        var handler = new CompleteSubtaskHandler(db);
        var getHandler = new GetSubtasksHandler(db);

        var todoId = await addTodo.HandleAsync("Main task");
        await addSubtask.HandleAsync(todoId, "Step one");
        var subtasks = await getHandler.HandleAsync([todoId]);
        var subtaskId = subtasks[todoId][0].Id;

        await handler.HandleAsync(subtaskId);

        var after = await getHandler.HandleAsync([todoId]);
        Assert.True(after[todoId][0].IsCompleted);
    }

    [Fact]
    public async Task CompleteSubtask_TogglesTwice_ReturnsFalse()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addSubtask = new AddSubtaskHandler(db);
        var handler = new CompleteSubtaskHandler(db);
        var getHandler = new GetSubtasksHandler(db);

        var todoId = await addTodo.HandleAsync("Main task");
        await addSubtask.HandleAsync(todoId, "Step one");
        var subtasks = await getHandler.HandleAsync([todoId]);
        var subtaskId = subtasks[todoId][0].Id;

        await handler.HandleAsync(subtaskId);
        await handler.HandleAsync(subtaskId);

        var after = await getHandler.HandleAsync([todoId]);
        Assert.False(after[todoId][0].IsCompleted);
    }

    [Fact]
    public async Task CompleteSubtask_NonExistentId_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new CompleteSubtaskHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(99999));
    }

    [Fact]
    public async Task CompleteSubtask_OnlyAffectsTargetSubtask()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addSubtask = new AddSubtaskHandler(db);
        var handler = new CompleteSubtaskHandler(db);
        var getHandler = new GetSubtasksHandler(db);

        var todoId = await addTodo.HandleAsync("Main task");
        await addSubtask.HandleAsync(todoId, "Step one");
        await addSubtask.HandleAsync(todoId, "Step two");

        var subtasks = await getHandler.HandleAsync([todoId]);
        await handler.HandleAsync(subtasks[todoId][0].Id);

        var after = await getHandler.HandleAsync([todoId]);
        Assert.True(after[todoId][0].IsCompleted);
        Assert.False(after[todoId][1].IsCompleted);
    }
}
