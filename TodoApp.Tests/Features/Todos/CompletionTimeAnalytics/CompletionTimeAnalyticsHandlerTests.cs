using TodoApp.Features.Todos;
using TodoApp.Features.Todos.ColorLabel;
using TodoApp.Features.Todos.CompletionTimeAnalytics;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.RecurringTodos;
using TodoApp.Features.Todos.TimeEstimates;
using Xunit;

namespace TodoApp.Tests.Features.Todos.CompletionTimeAnalytics;

public class CompletionTimeAnalyticsHandlerTests
{
    private readonly CompletionTimeAnalyticsHandler _handler = new();

    private static TodoSummary MakeTodo(int id, bool isCompleted, DateTime createdAt, DateTime? completedAt = null, TodoPriority priority = TodoPriority.None) =>
        new(id, $"Todo {id}", isCompleted, createdAt, priority, null, false, null, RecurrenceRule.None, 1, completedAt, TimeEstimate.None, TodoColorLabel.None);

    [Fact]
    public void Handle_NoTodos_ReturnsNullOverallAverage()
    {
        var result = _handler.Handle([]);

        Assert.Null(result.OverallAverageDays);
        Assert.Equal(0, result.SampleCount);
    }

    [Fact]
    public void Handle_OnlyActiveTodos_ReturnsNullOverallAverage()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, isCompleted: false, createdAt: DateTime.Today.AddDays(-5)),
            MakeTodo(2, isCompleted: false, createdAt: DateTime.Today.AddDays(-2)),
        };

        var result = _handler.Handle(todos);

        Assert.Null(result.OverallAverageDays);
        Assert.Equal(0, result.SampleCount);
    }

    [Fact]
    public void Handle_CompletedTodoWithNoCompletedAt_ExcludedFromAverage()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, isCompleted: true, createdAt: DateTime.Today.AddDays(-5), completedAt: null),
        };

        var result = _handler.Handle(todos);

        Assert.Null(result.OverallAverageDays);
        Assert.Equal(0, result.SampleCount);
    }

    [Fact]
    public void Handle_SingleCompletedTodo_ReturnsCorrectDays()
    {
        var created = DateTime.Today.AddDays(-4);
        var completed = DateTime.Today;
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, isCompleted: true, createdAt: created, completedAt: completed),
        };

        var result = _handler.Handle(todos);

        Assert.Equal(4.0, result.OverallAverageDays);
        Assert.Equal(1, result.SampleCount);
    }

    [Fact]
    public void Handle_MultipleCompletedTodos_AveragesCorrectly()
    {
        var now = DateTime.Today;
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, isCompleted: true, createdAt: now.AddDays(-2), completedAt: now),   // 2 days
            MakeTodo(2, isCompleted: true, createdAt: now.AddDays(-4), completedAt: now),   // 4 days
        };

        var result = _handler.Handle(todos);

        Assert.Equal(3.0, result.OverallAverageDays); // (2+4)/2 = 3
        Assert.Equal(2, result.SampleCount);
    }

    [Fact]
    public void Handle_ByPriority_ComputesSeparateAverages()
    {
        var now = DateTime.Today;
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, isCompleted: true, createdAt: now.AddDays(-1), completedAt: now, priority: TodoPriority.High),
            MakeTodo(2, isCompleted: true, createdAt: now.AddDays(-3), completedAt: now, priority: TodoPriority.Medium),
            MakeTodo(3, isCompleted: true, createdAt: now.AddDays(-7), completedAt: now, priority: TodoPriority.Low),
        };

        var result = _handler.Handle(todos);

        Assert.Equal(1.0, result.HighPriorityAverageDays);
        Assert.Equal(3.0, result.MediumPriorityAverageDays);
        Assert.Equal(7.0, result.LowPriorityAverageDays);
    }

    [Fact]
    public void Handle_NoPriorityTodos_PriorityAveragesAreNull()
    {
        var now = DateTime.Today;
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, isCompleted: true, createdAt: now.AddDays(-5), completedAt: now, priority: TodoPriority.None),
        };

        var result = _handler.Handle(todos);

        Assert.NotNull(result.OverallAverageDays); // counted in overall
        Assert.Null(result.HighPriorityAverageDays);
        Assert.Null(result.MediumPriorityAverageDays);
        Assert.Null(result.LowPriorityAverageDays);
    }

    [Fact]
    public void Handle_CompletedBeforeCreated_ClampedToZero()
    {
        // Guard against clock skew producing negative days
        var now = DateTime.Today;
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, isCompleted: true, createdAt: now, completedAt: now.AddDays(-1)),
        };

        var result = _handler.Handle(todos);

        Assert.Equal(0.0, result.OverallAverageDays);
    }

    [Fact]
    public void Handle_MixedActiveAndCompleted_OnlyCountsCompleted()
    {
        var now = DateTime.Today;
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, isCompleted: true, createdAt: now.AddDays(-6), completedAt: now),
            MakeTodo(2, isCompleted: false, createdAt: now.AddDays(-100)), // active — should not affect average
        };

        var result = _handler.Handle(todos);

        Assert.Equal(6.0, result.OverallAverageDays);
        Assert.Equal(1, result.SampleCount);
    }

    [Fact]
    public void Handle_RoundsToOneDecimalPlace()
    {
        var now = DateTime.Today;
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, isCompleted: true, createdAt: now.AddDays(-1), completedAt: now),
            MakeTodo(2, isCompleted: true, createdAt: now.AddDays(-2), completedAt: now),
            MakeTodo(3, isCompleted: true, createdAt: now.AddDays(-3), completedAt: now),
        };

        var result = _handler.Handle(todos);

        // (1+2+3)/3 = 2.0
        Assert.Equal(2.0, result.OverallAverageDays);
        // Verify rounding: 1/3 scenarios
        var oddTodos = new List<TodoSummary>
        {
            MakeTodo(4, isCompleted: true, createdAt: now.AddDays(-1), completedAt: now),
            MakeTodo(5, isCompleted: true, createdAt: now.AddDays(-2), completedAt: now),
        };
        var oddResult = _handler.Handle(oddTodos);
        Assert.Equal(1.5, oddResult.OverallAverageDays);
    }
}
