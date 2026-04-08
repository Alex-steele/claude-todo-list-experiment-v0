using TodoApp.Features.Todos;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.GetTodosStats;
using Xunit;

namespace TodoApp.Tests.Features.Todos.GetTodosStats;

public class GetTodosStatsHandlerTests
{
    private readonly GetTodosStatsHandler _handler = new();

    private static TodoSummary MakeTodo(int id, bool isCompleted = false, DateTime? dueDate = null) =>
        new(id, $"Todo {id}", isCompleted, DateTime.UtcNow, TodoPriority.None, dueDate);

    [Fact]
    public void EmptyList_ReturnsAllZeros()
    {
        var stats = _handler.Handle([]);

        Assert.Equal(0, stats.Total);
        Assert.Equal(0, stats.Active);
        Assert.Equal(0, stats.Completed);
        Assert.Equal(0, stats.Overdue);
        Assert.Equal(0, stats.CompletionPercentage);
    }

    [Fact]
    public void AllActive_ReturnsCorrectCounts()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1),
            MakeTodo(2),
            MakeTodo(3),
        };

        var stats = _handler.Handle(todos);

        Assert.Equal(3, stats.Total);
        Assert.Equal(3, stats.Active);
        Assert.Equal(0, stats.Completed);
        Assert.Equal(0, stats.CompletionPercentage);
    }

    [Fact]
    public void AllCompleted_Returns100Percent()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, isCompleted: true),
            MakeTodo(2, isCompleted: true),
        };

        var stats = _handler.Handle(todos);

        Assert.Equal(2, stats.Total);
        Assert.Equal(0, stats.Active);
        Assert.Equal(2, stats.Completed);
        Assert.Equal(100, stats.CompletionPercentage);
    }

    [Fact]
    public void MixedCompletions_ReturnsCorrectPercentage()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, isCompleted: true),
            MakeTodo(2, isCompleted: false),
            MakeTodo(3, isCompleted: false),
            MakeTodo(4, isCompleted: false),
        };

        var stats = _handler.Handle(todos);

        Assert.Equal(4, stats.Total);
        Assert.Equal(3, stats.Active);
        Assert.Equal(1, stats.Completed);
        Assert.Equal(25, stats.CompletionPercentage);
    }

    [Fact]
    public void OverdueTodos_CountedCorrectly()
    {
        var pastDate = DateTime.Today.AddDays(-1);
        var futureDate = DateTime.Today.AddDays(3);

        var todos = new List<TodoSummary>
        {
            MakeTodo(1, dueDate: pastDate),                          // overdue
            MakeTodo(2, dueDate: pastDate),                          // overdue
            MakeTodo(3, dueDate: futureDate),                        // not overdue
            MakeTodo(4, isCompleted: true, dueDate: pastDate),       // completed, not counted
            MakeTodo(5),                                             // no due date
        };

        var stats = _handler.Handle(todos);

        Assert.Equal(2, stats.Overdue);
    }

    [Fact]
    public void CompletedOverdueTodos_NotCountedAsOverdue()
    {
        var pastDate = DateTime.Today.AddDays(-5);
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, isCompleted: true, dueDate: pastDate),
        };

        var stats = _handler.Handle(todos);

        Assert.Equal(0, stats.Overdue);
    }
}
