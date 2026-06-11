using TodoApp.Features.Todos;
using TodoApp.Features.Todos.FocusMode;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.RecurringTodos;
using TodoApp.Features.Todos.TimeEstimates;
using Xunit;

namespace TodoApp.Tests.Features.Todos.FocusMode;

public class FocusModeHandlerTests
{
    private static TodoSummary MakeTodo(int id, string title, bool isCompleted = false,
        TodoPriority priority = TodoPriority.None, DateTime? dueDate = null, bool isPinned = false)
        => new(id, title, isCompleted, DateTime.UtcNow, priority, dueDate, isPinned);

    private readonly FocusModeHandler _handler = new();

    [Fact]
    public void EmptyList_ReturnsEmpty()
    {
        var result = _handler.Handle([]);
        Assert.Empty(result);
    }

    [Fact]
    public void CompletedTodos_AreExcluded()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, "Done", isCompleted: true, priority: TodoPriority.High),
            MakeTodo(2, "Also done", isCompleted: true, dueDate: DateTime.Today)
        };

        var result = _handler.Handle(todos);

        Assert.Empty(result);
    }

    [Fact]
    public void HighPriorityActiveTodo_IsIncluded()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, "High priority", priority: TodoPriority.High)
        };

        var result = _handler.Handle(todos);

        Assert.Single(result);
        Assert.Equal("High priority", result[0].Title);
    }

    [Fact]
    public void MediumPriorityActiveTodo_IsIncluded()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, "Medium priority", priority: TodoPriority.Medium)
        };

        var result = _handler.Handle(todos);

        Assert.Single(result);
    }

    [Fact]
    public void LowPriorityWithNoDueDateAndNotPinned_IsExcluded()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, "Low and no date", priority: TodoPriority.Low)
        };

        var result = _handler.Handle(todos);

        Assert.Empty(result);
    }

    [Fact]
    public void NoPriorityWithNoDueDate_IsExcluded()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, "Someday maybe")
        };

        var result = _handler.Handle(todos);

        Assert.Empty(result);
    }

    [Fact]
    public void OverdueTodo_IsIncluded()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, "Overdue", dueDate: DateTime.Today.AddDays(-1))
        };

        var result = _handler.Handle(todos);

        Assert.Single(result);
    }

    [Fact]
    public void DueTodayTodo_IsIncluded()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, "Due today", dueDate: DateTime.Today)
        };

        var result = _handler.Handle(todos);

        Assert.Single(result);
    }

    [Fact]
    public void FutureDueDateOnly_IsExcluded()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, "Due next week", dueDate: DateTime.Today.AddDays(7))
        };

        var result = _handler.Handle(todos);

        Assert.Empty(result);
    }

    [Fact]
    public void PinnedTodo_IsIncluded_RegardlessOfPriority()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, "Pinned low priority", priority: TodoPriority.Low, isPinned: true)
        };

        var result = _handler.Handle(todos);

        Assert.Single(result);
    }

    [Fact]
    public void PinnedTodo_IsSortedFirst()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, "High priority", priority: TodoPriority.High),
            MakeTodo(2, "Overdue", dueDate: DateTime.Today.AddDays(-2)),
            MakeTodo(3, "Pinned", isPinned: true, priority: TodoPriority.Low)
        };

        var result = _handler.Handle(todos);

        Assert.Equal("Pinned", result[0].Title);
    }

    [Fact]
    public void OverdueTodo_IsSortedBeforeDueToday()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, "Due today", dueDate: DateTime.Today),
            MakeTodo(2, "Overdue", dueDate: DateTime.Today.AddDays(-1))
        };

        var result = _handler.Handle(todos);

        Assert.Equal("Overdue", result[0].Title);
        Assert.Equal("Due today", result[1].Title);
    }

    [Fact]
    public void WithinSameDateTier_HighPriority_IsSortedFirst()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, "Medium no date", priority: TodoPriority.Medium),
            MakeTodo(2, "High no date", priority: TodoPriority.High)
        };

        var result = _handler.Handle(todos);

        Assert.Equal("High no date", result[0].Title);
        Assert.Equal("Medium no date", result[1].Title);
    }

    [Fact]
    public void MixedList_ReturnsOnlyFocusItems()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, "Someday", priority: TodoPriority.None),
            MakeTodo(2, "Urgent", priority: TodoPriority.High),
            MakeTodo(3, "Completed high", isCompleted: true, priority: TodoPriority.High),
            MakeTodo(4, "Future low", priority: TodoPriority.Low, dueDate: DateTime.Today.AddDays(5)),
            MakeTodo(5, "Overdue", dueDate: DateTime.Today.AddDays(-1))
        };

        var result = _handler.Handle(todos);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Title == "Urgent");
        Assert.Contains(result, t => t.Title == "Overdue");
    }
}
