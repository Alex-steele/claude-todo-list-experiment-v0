using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.Subtasks;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Subtasks;

public class EditSubtaskHandlerTests
{
    [Fact]
    public async Task EditSubtask_ValidTitle_UpdatesTitle()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addSubtask = new AddSubtaskHandler(db);
        var handler = new EditSubtaskHandler(db);
        var getSubtasks = new GetSubtasksHandler(db);

        var todoId = await addTodo.HandleAsync("Parent task");
        var subtaskId = await addSubtask.HandleAsync(todoId, "Original title");

        await handler.HandleAsync(subtaskId, "Renamed title");

        var subtasks = await getSubtasks.HandleAsync([todoId]);
        Assert.Equal("Renamed title", subtasks[todoId][0].Title);
    }

    [Fact]
    public async Task EditSubtask_EmptyTitle_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addSubtask = new AddSubtaskHandler(db);
        var handler = new EditSubtaskHandler(db);

        var todoId = await addTodo.HandleAsync("Parent task");
        var subtaskId = await addSubtask.HandleAsync(todoId, "Step one");

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(subtaskId, ""));
        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(subtaskId, "   "));
    }

    [Fact]
    public async Task EditSubtask_WhitespaceTitle_IsTrimmed()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addSubtask = new AddSubtaskHandler(db);
        var handler = new EditSubtaskHandler(db);
        var getSubtasks = new GetSubtasksHandler(db);

        var todoId = await addTodo.HandleAsync("Parent task");
        var subtaskId = await addSubtask.HandleAsync(todoId, "Original");

        await handler.HandleAsync(subtaskId, "  Trimmed title  ");

        var subtasks = await getSubtasks.HandleAsync([todoId]);
        Assert.Equal("Trimmed title", subtasks[todoId][0].Title);
    }

    [Fact]
    public async Task EditSubtask_CompletionStatusPreserved()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addSubtask = new AddSubtaskHandler(db);
        var completeSubtask = new CompleteSubtaskHandler(db);
        var handler = new EditSubtaskHandler(db);
        var getSubtasks = new GetSubtasksHandler(db);

        var todoId = await addTodo.HandleAsync("Parent task");
        var subtaskId = await addSubtask.HandleAsync(todoId, "Step one");
        await completeSubtask.HandleAsync(subtaskId);

        await handler.HandleAsync(subtaskId, "Step one renamed");

        var subtasks = await getSubtasks.HandleAsync([todoId]);
        Assert.True(subtasks[todoId][0].IsCompleted);
        Assert.Equal("Step one renamed", subtasks[todoId][0].Title);
    }

    [Fact]
    public async Task EditSubtask_MultipleSubtasks_OnlyTargetUpdated()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addSubtask = new AddSubtaskHandler(db);
        var handler = new EditSubtaskHandler(db);
        var getSubtasks = new GetSubtasksHandler(db);

        var todoId = await addTodo.HandleAsync("Parent task");
        var id1 = await addSubtask.HandleAsync(todoId, "Step one");
        var id2 = await addSubtask.HandleAsync(todoId, "Step two");

        await handler.HandleAsync(id1, "Step one updated");

        var subtasks = await getSubtasks.HandleAsync([todoId]);
        Assert.Equal("Step one updated", subtasks[todoId][0].Title);
        Assert.Equal("Step two", subtasks[todoId][1].Title);
    }
}
