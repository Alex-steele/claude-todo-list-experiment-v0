using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.Subtasks;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Subtasks;

public class DeleteSubtaskHandlerTests
{
    [Fact]
    public async Task DeleteSubtask_ExistingSubtask_IsRemoved()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addSubtask = new AddSubtaskHandler(db);
        var handler = new DeleteSubtaskHandler(db);
        var getHandler = new GetSubtasksHandler(db);

        var todoId = await addTodo.HandleAsync("Main task");
        await addSubtask.HandleAsync(todoId, "Step one");

        var before = await getHandler.HandleAsync([todoId]);
        var subtaskId = before[todoId][0].Id;

        await handler.HandleAsync(subtaskId);

        var after = await getHandler.HandleAsync([todoId]);
        Assert.False(after.ContainsKey(todoId));
    }

    [Fact]
    public async Task DeleteSubtask_NonExistentId_DoesNotThrow()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new DeleteSubtaskHandler(db);

        await handler.HandleAsync(99999);
    }

    [Fact]
    public async Task DeleteSubtask_OnlyRemovesTargetSubtask()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addSubtask = new AddSubtaskHandler(db);
        var handler = new DeleteSubtaskHandler(db);
        var getHandler = new GetSubtasksHandler(db);

        var todoId = await addTodo.HandleAsync("Main task");
        await addSubtask.HandleAsync(todoId, "Step one");
        await addSubtask.HandleAsync(todoId, "Step two");

        var before = await getHandler.HandleAsync([todoId]);
        await handler.HandleAsync(before[todoId][0].Id);

        var after = await getHandler.HandleAsync([todoId]);
        Assert.Single(after[todoId]);
        Assert.Equal("Step two", after[todoId][0].Title);
    }
}
