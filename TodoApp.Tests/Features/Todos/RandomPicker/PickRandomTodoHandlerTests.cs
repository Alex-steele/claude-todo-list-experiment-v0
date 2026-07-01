using TodoApp.Features.Todos;
using TodoApp.Features.Todos.ColorLabel;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.RandomPicker;
using TodoApp.Features.Todos.RecurringTodos;
using TodoApp.Features.Todos.TimeEstimates;
using Xunit;

namespace TodoApp.Tests.Features.Todos.RandomPicker;

public class PickRandomTodoHandlerTests
{
    private readonly PickRandomTodoHandler _handler = new();

    private static TodoSummary MakeTodo(int id, bool completed = false) =>
        new(id, $"Todo {id}", completed, DateTime.UtcNow, TodoPriority.None, null);

    [Fact]
    public void Handle_EmptyList_ReturnsNull()
    {
        var result = _handler.Handle([]);
        Assert.Null(result);
    }

    [Fact]
    public void Handle_AllCompleted_ReturnsNull()
    {
        var todos = new[] { MakeTodo(1, completed: true), MakeTodo(2, completed: true) };
        var result = _handler.Handle(todos);
        Assert.Null(result);
    }

    [Fact]
    public void Handle_SingleActiveTodo_ReturnsThatTodo()
    {
        var todos = new[] { MakeTodo(1) };
        var result = _handler.Handle(todos);
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public void Handle_MixedTodos_ReturnsOnlyActive()
    {
        var todos = new[] { MakeTodo(1, completed: true), MakeTodo(2), MakeTodo(3) };
        for (var i = 0; i < 20; i++)
        {
            var result = _handler.Handle(todos);
            Assert.NotNull(result);
            Assert.False(result.IsCompleted);
            Assert.NotEqual(1, result.Id);
        }
    }

    [Fact]
    public void Handle_WithExcludeId_NeverReturnsExcluded()
    {
        var todos = new[] { MakeTodo(1), MakeTodo(2), MakeTodo(3) };
        for (var i = 0; i < 30; i++)
        {
            var result = _handler.Handle(todos, excludeId: 1);
            Assert.NotNull(result);
            Assert.NotEqual(1, result.Id);
        }
    }

    [Fact]
    public void Handle_WithExcludeId_WhenOnlyOneLeft_ReturnsNull()
    {
        var todos = new[] { MakeTodo(1) };
        var result = _handler.Handle(todos, excludeId: 1);
        Assert.Null(result);
    }

    [Fact]
    public void Handle_WithMultipleTodos_EventuallyPicksDifferentItems()
    {
        var todos = Enumerable.Range(1, 5).Select(i => MakeTodo(i)).ToList();
        var pickedIds = new HashSet<int>();
        for (var i = 0; i < 50; i++)
        {
            var result = _handler.Handle(todos);
            if (result is not null) pickedIds.Add(result.Id);
        }
        // With 50 picks from 5 items, we should see at least 2 distinct picks
        Assert.True(pickedIds.Count >= 2, $"Expected at least 2 distinct picks, got {pickedIds.Count}: [{string.Join(",", pickedIds)}]");
    }

    [Fact]
    public void Handle_ReturnedTodo_IsFromInputList()
    {
        var todos = new[] { MakeTodo(10), MakeTodo(20), MakeTodo(30) };
        var validIds = todos.Select(t => t.Id).ToHashSet();
        for (var i = 0; i < 20; i++)
        {
            var result = _handler.Handle(todos);
            Assert.NotNull(result);
            Assert.Contains(result.Id, validIds);
        }
    }
}
