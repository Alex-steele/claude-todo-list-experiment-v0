using TodoApp.Features.Todos;
using TodoApp.Features.Todos.ColorLabel;
using TodoApp.Features.Todos.DueSummary;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.RecurringTodos;
using TodoApp.Features.Todos.TimeEstimates;
using Xunit;

namespace TodoApp.Tests.Features.Todos.DueSummary;

public class DueSummaryHandlerTests
{
    private readonly DueSummaryHandler _handler = new();
    private static readonly DateTime Today = DateTime.Today;

    private static TodoSummary MakeTodo(int id, bool completed = false, DateTime? dueDate = null) =>
        new(id, $"Todo {id}", completed, DateTime.UtcNow, TodoPriority.None, dueDate);

    [Fact]
    public void Handle_EmptyList_ReturnsZeros()
    {
        var result = _handler.Handle([]);
        Assert.Equal(0, result.Overdue);
        Assert.Equal(0, result.DueToday);
    }

    [Fact]
    public void Handle_NoDueDates_ReturnsZeros()
    {
        var todos = new[] { MakeTodo(1), MakeTodo(2) };
        var result = _handler.Handle(todos);
        Assert.Equal(0, result.Overdue);
        Assert.Equal(0, result.DueToday);
    }

    [Fact]
    public void Handle_OverdueTodo_CountsAsOverdue()
    {
        var todos = new[] { MakeTodo(1, dueDate: Today.AddDays(-1)) };
        var result = _handler.Handle(todos);
        Assert.Equal(1, result.Overdue);
        Assert.Equal(0, result.DueToday);
    }

    [Fact]
    public void Handle_DueTodayTodo_CountsAsDueToday()
    {
        var todos = new[] { MakeTodo(1, dueDate: Today) };
        var result = _handler.Handle(todos);
        Assert.Equal(0, result.Overdue);
        Assert.Equal(1, result.DueToday);
    }

    [Fact]
    public void Handle_FutureTodo_CountsAsNeither()
    {
        var todos = new[] { MakeTodo(1, dueDate: Today.AddDays(1)) };
        var result = _handler.Handle(todos);
        Assert.Equal(0, result.Overdue);
        Assert.Equal(0, result.DueToday);
    }

    [Fact]
    public void Handle_CompletedOverdueTodo_IsIgnored()
    {
        var todos = new[] { MakeTodo(1, completed: true, dueDate: Today.AddDays(-2)) };
        var result = _handler.Handle(todos);
        Assert.Equal(0, result.Overdue);
    }

    [Fact]
    public void Handle_CompletedDueTodayTodo_IsIgnored()
    {
        var todos = new[] { MakeTodo(1, completed: true, dueDate: Today) };
        var result = _handler.Handle(todos);
        Assert.Equal(0, result.DueToday);
    }

    [Fact]
    public void Handle_MixedTodos_CountsCorrectly()
    {
        var todos = new[]
        {
            MakeTodo(1, dueDate: Today.AddDays(-3)),   // overdue
            MakeTodo(2, dueDate: Today.AddDays(-1)),   // overdue
            MakeTodo(3, dueDate: Today),               // due today
            MakeTodo(4, dueDate: Today.AddDays(1)),    // future — ignored
            MakeTodo(5),                                // no date — ignored
            MakeTodo(6, completed: true, dueDate: Today.AddDays(-5)) // completed overdue — ignored
        };
        var result = _handler.Handle(todos);
        Assert.Equal(2, result.Overdue);
        Assert.Equal(1, result.DueToday);
    }

    [Fact]
    public void Handle_MultipleOverdue_CountsAll()
    {
        var todos = Enumerable.Range(1, 5)
            .Select(i => MakeTodo(i, dueDate: Today.AddDays(-i)))
            .ToArray();
        var result = _handler.Handle(todos);
        Assert.Equal(5, result.Overdue);
    }
}
