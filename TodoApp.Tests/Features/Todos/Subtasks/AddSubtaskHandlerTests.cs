using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.Subtasks;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Subtasks;

public class AddSubtaskHandlerTests
{
    [Fact]
    public async Task AddSubtask_ValidTitle_ReturnsPositiveId()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var handler = new AddSubtaskHandler(db);

        var todoId = await addTodo.HandleAsync("Main task");
        var subtaskId = await handler.HandleAsync(todoId, "Step one");

        Assert.True(subtaskId > 0);
    }

    [Fact]
    public async Task AddSubtask_EmptyTitle_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var handler = new AddSubtaskHandler(db);

        var todoId = await addTodo.HandleAsync("Main task");

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(todoId, ""));
        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(todoId, "   "));
    }

    [Fact]
    public async Task AddSubtask_WhitespaceTitle_IsTrimmed()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var handler = new AddSubtaskHandler(db);
        var getHandler = new GetSubtasksHandler(db);

        var todoId = await addTodo.HandleAsync("Main task");
        await handler.HandleAsync(todoId, "  Step one  ");

        var subtasks = await getHandler.HandleAsync([todoId]);
        Assert.Equal("Step one", subtasks[todoId][0].Title);
    }

    [Fact]
    public async Task AddSubtask_MultipleSubtasks_AllStoredInOrder()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var handler = new AddSubtaskHandler(db);
        var getHandler = new GetSubtasksHandler(db);

        var todoId = await addTodo.HandleAsync("Main task");
        await handler.HandleAsync(todoId, "Step one");
        await handler.HandleAsync(todoId, "Step two");
        await handler.HandleAsync(todoId, "Step three");

        var subtasks = await getHandler.HandleAsync([todoId]);
        Assert.Equal(3, subtasks[todoId].Count);
        Assert.Equal("Step one", subtasks[todoId][0].Title);
        Assert.Equal("Step three", subtasks[todoId][2].Title);
    }

    [Fact]
    public async Task AddSubtask_IsNotCompletedByDefault()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var handler = new AddSubtaskHandler(db);
        var getHandler = new GetSubtasksHandler(db);

        var todoId = await addTodo.HandleAsync("Main task");
        await handler.HandleAsync(todoId, "Step one");

        var subtasks = await getHandler.HandleAsync([todoId]);
        Assert.False(subtasks[todoId][0].IsCompleted);
    }
}
