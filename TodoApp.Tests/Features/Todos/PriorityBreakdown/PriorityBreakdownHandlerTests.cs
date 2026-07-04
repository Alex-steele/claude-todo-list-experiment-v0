using TodoApp.Features.Todos;
using TodoApp.Features.Todos.ColorLabel;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.PriorityBreakdown;
using TodoApp.Features.Todos.RecurringTodos;
using TodoApp.Features.Todos.TimeEstimates;
using Xunit;

namespace TodoApp.Tests.Features.Todos.PriorityBreakdown;

public class PriorityBreakdownHandlerTests
{
    private readonly PriorityBreakdownHandler _handler = new();

    private static TodoSummary MakeTodo(int id, bool isCompleted, TodoPriority priority) =>
        new(id, $"Todo {id}", isCompleted, DateTime.UtcNow, priority, null, false, null,
            RecurrenceRule.None, 1, isCompleted ? DateTime.UtcNow : null, TimeEstimate.None, TodoColorLabel.None);

    [Fact]
    public void Handle_EmptyList_HasDataFalse()
    {
        var result = _handler.Handle([]);

        Assert.False(result.HasData);
        Assert.Equal(0, result.High.Total);
        Assert.Equal(0, result.Medium.Total);
        Assert.Equal(0, result.Low.Total);
    }

    [Fact]
    public void Handle_OnlyNoPriorityTodos_HasDataFalse()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, false, TodoPriority.None),
            MakeTodo(2, true,  TodoPriority.None),
        };

        var result = _handler.Handle(todos);

        Assert.False(result.HasData);
    }

    [Fact]
    public void Handle_HighPriorityActive_CountedCorrectly()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, false, TodoPriority.High),
            MakeTodo(2, false, TodoPriority.High),
        };

        var result = _handler.Handle(todos);

        Assert.True(result.HasData);
        Assert.Equal(2, result.High.Active);
        Assert.Equal(0, result.High.Completed);
        Assert.Equal(2, result.High.Total);
        Assert.Equal(0, result.High.CompletionPercent);
    }

    [Fact]
    public void Handle_HighPriorityAllCompleted_Returns100Percent()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, true, TodoPriority.High),
            MakeTodo(2, true, TodoPriority.High),
        };

        var result = _handler.Handle(todos);

        Assert.Equal(0, result.High.Active);
        Assert.Equal(2, result.High.Completed);
        Assert.Equal(100, result.High.CompletionPercent);
    }

    [Fact]
    public void Handle_MixedPriorityAndCompletion_EachPriorityIndependent()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, false, TodoPriority.High),
            MakeTodo(2, true,  TodoPriority.High),   // 1 active, 1 done → 50%
            MakeTodo(3, false, TodoPriority.Medium),
            MakeTodo(4, false, TodoPriority.Medium),
            MakeTodo(5, true,  TodoPriority.Medium),  // 2 active, 1 done → 33%
            MakeTodo(6, true,  TodoPriority.Low),     // 0 active, 1 done → 100%
        };

        var result = _handler.Handle(todos);

        Assert.True(result.HasData);

        Assert.Equal(1, result.High.Active);
        Assert.Equal(1, result.High.Completed);
        Assert.Equal(50, result.High.CompletionPercent);

        Assert.Equal(2, result.Medium.Active);
        Assert.Equal(1, result.Medium.Completed);
        Assert.Equal(33, result.Medium.CompletionPercent);

        Assert.Equal(0, result.Low.Active);
        Assert.Equal(1, result.Low.Completed);
        Assert.Equal(100, result.Low.CompletionPercent);
    }

    [Fact]
    public void Handle_NoPriorityTodosDoNotAffectBreakdown()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, false, TodoPriority.None),
            MakeTodo(2, true,  TodoPriority.None),
            MakeTodo(3, false, TodoPriority.High),
        };

        var result = _handler.Handle(todos);

        Assert.Equal(1, result.High.Active);
        Assert.Equal(0, result.Medium.Total);
        Assert.Equal(0, result.Low.Total);
    }

    [Fact]
    public void Handle_SingleTodo_HasDataTrue()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, false, TodoPriority.Medium),
        };

        var result = _handler.Handle(todos);

        Assert.True(result.HasData);
        Assert.Equal(1, result.Medium.Active);
        Assert.Equal(0, result.Medium.Completed);
    }

    [Fact]
    public void PriorityStats_CompletionPercent_TruncatesNotRounds()
    {
        // 1 out of 3 = 33.33… → should be 33 (integer truncation)
        var stats = new PriorityStats(Active: 2, Completed: 1);
        Assert.Equal(33, stats.CompletionPercent);
    }
}
