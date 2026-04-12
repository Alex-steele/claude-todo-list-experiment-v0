using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.Subtasks;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Subtasks;

public class GetSubtasksHandlerTests
{
    [Fact]
    public async Task GetSubtasks_EmptyIdList_ReturnsEmptyDictionary()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new GetSubtasksHandler(db);

        var result = await handler.HandleAsync([]);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetSubtasks_TodoWithNoSubtasks_NotInResult()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var handler = new GetSubtasksHandler(db);

        var todoId = await addTodo.HandleAsync("Main task");

        var result = await handler.HandleAsync([todoId]);

        Assert.False(result.ContainsKey(todoId));
    }

    [Fact]
    public async Task GetSubtasks_GroupsSubtasksByTodoId()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addSubtask = new AddSubtaskHandler(db);
        var handler = new GetSubtasksHandler(db);

        var todo1 = await addTodo.HandleAsync("Todo 1");
        var todo2 = await addTodo.HandleAsync("Todo 2");

        await addSubtask.HandleAsync(todo1, "Step A");
        await addSubtask.HandleAsync(todo1, "Step B");
        await addSubtask.HandleAsync(todo2, "Step C");

        var result = await handler.HandleAsync([todo1, todo2]);

        Assert.Equal(2, result[todo1].Count);
        Assert.Single(result[todo2]);
        Assert.Equal("Step C", result[todo2][0].Title);
    }

    [Fact]
    public async Task GetSubtasks_ReturnsSubtasksInInsertionOrder()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addSubtask = new AddSubtaskHandler(db);
        var handler = new GetSubtasksHandler(db);

        var todoId = await addTodo.HandleAsync("Main task");
        await addSubtask.HandleAsync(todoId, "First");
        await addSubtask.HandleAsync(todoId, "Second");
        await addSubtask.HandleAsync(todoId, "Third");

        var result = await handler.HandleAsync([todoId]);
        var titles = result[todoId].Select(s => s.Title).ToList();

        Assert.Equal(["First", "Second", "Third"], titles);
    }
}
