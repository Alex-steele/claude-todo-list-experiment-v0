using TodoApp.Features.Todos;
using TodoApp.Features.Todos.ColorLabel;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.RecurringTodos;
using TodoApp.Features.Todos.SuggestNext;
using TodoApp.Features.Todos.TimeEstimates;
using Xunit;

namespace TodoApp.Tests.Features.Todos.SuggestNext;

public class SuggestNextTodoHandlerTests
{
    private readonly SuggestNextTodoHandler _handler = new();

    private static TodoSummary MakeTodo(
        int id,
        TodoPriority priority = TodoPriority.None,
        DateTime? dueDate = null,
        bool isCompleted = false,
        int listId = 1,
        bool isBlocked = false,
        int? dependsOnTodoId = null,
        DateTime? createdAt = null) =>
        new(id, $"Todo {id}", isCompleted, createdAt ?? DateTime.Today.AddDays(-10), priority, dueDate,
            false, null, RecurrenceRule.None, listId, null, TimeEstimate.None, TodoColorLabel.None,
            isBlocked, null, 0, null, dependsOnTodoId);

    [Fact]
    public void Handle_NoTodos_ReturnsNull()
    {
        var result = _handler.Handle([], listId: 1);

        Assert.Null(result);
    }

    [Fact]
    public void Handle_AllCompleted_ReturnsNull()
    {
        var todos = new List<TodoSummary> { MakeTodo(1, isCompleted: true) };

        var result = _handler.Handle(todos, listId: 1);

        Assert.Null(result);
    }

    [Fact]
    public void Handle_TodosFromOtherLists_ExcludedFromCandidates()
    {
        var todos = new List<TodoSummary> { MakeTodo(1, listId: 2) };

        var result = _handler.Handle(todos, listId: 1);

        Assert.Null(result);
    }

    [Fact]
    public void Handle_ManuallyBlockedTodo_Excluded()
    {
        var todos = new List<TodoSummary> { MakeTodo(1, isBlocked: true) };

        var result = _handler.Handle(todos, listId: 1);

        Assert.Null(result);
    }

    [Fact]
    public void Handle_BlockedByIncompleteDependency_ExcludedInFavorOfUnblockedTodo()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, dependsOnTodoId: 2),
            MakeTodo(2, isCompleted: false)
        };

        var result = _handler.Handle(todos, listId: 1);

        // Todo 1 is blocked until todo 2 is done, so todo 2 (unblocked) is suggested instead.
        Assert.Equal(2, result!.Todo.Id);
    }

    [Fact]
    public void Handle_OnlyCandidateIsBlockedByIncompleteDependency_ReturnsNull()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, dependsOnTodoId: 2),
            MakeTodo(2, isCompleted: false, isBlocked: true)
        };

        var result = _handler.Handle(todos, listId: 1);

        Assert.Null(result);
    }

    [Fact]
    public void Handle_DependencyCompleted_NoLongerBlocked()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, dependsOnTodoId: 2),
            MakeTodo(2, isCompleted: true)
        };

        var result = _handler.Handle(todos, listId: 1);

        Assert.NotNull(result);
        Assert.Equal(1, result!.Todo.Id);
    }

    [Fact]
    public void Handle_OverdueTodo_PreferredOverNonOverdue()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, dueDate: DateTime.Today.AddDays(5)),
            MakeTodo(2, dueDate: DateTime.Today.AddDays(-1))
        };

        var result = _handler.Handle(todos, listId: 1);

        Assert.Equal(2, result!.Todo.Id);
        Assert.Contains("overdue", result.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Handle_DueTodayTodo_PreferredOverNonDueTodos()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, dueDate: DateTime.Today.AddDays(3)),
            MakeTodo(2, dueDate: DateTime.Today)
        };

        var result = _handler.Handle(todos, listId: 1);

        Assert.Equal(2, result!.Todo.Id);
        Assert.Contains("due today", result.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Handle_AmongEquallyUrgentTodos_HighPriorityWins()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, priority: TodoPriority.Low, dueDate: DateTime.Today.AddDays(-1)),
            MakeTodo(2, priority: TodoPriority.High, dueDate: DateTime.Today.AddDays(-1))
        };

        var result = _handler.Handle(todos, listId: 1);

        Assert.Equal(2, result!.Todo.Id);
        Assert.Contains("high priority", result.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Handle_NoUrgencyOrPriority_TiebreaksOnOldestCreatedAt()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, createdAt: DateTime.Today.AddDays(-1)),
            MakeTodo(2, createdAt: DateTime.Today.AddDays(-5))
        };

        var result = _handler.Handle(todos, listId: 1);

        Assert.Equal(2, result!.Todo.Id);
    }

    [Fact]
    public void Handle_NoUrgencyOrPriority_ReasonMentionsOldest()
    {
        var todos = new List<TodoSummary> { MakeTodo(1, priority: TodoPriority.None, dueDate: null) };

        var result = _handler.Handle(todos, listId: 1);

        Assert.NotNull(result);
        Assert.Contains("oldest", result!.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Handle_OverdueAndHighPriority_ReasonMentionsBoth()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, priority: TodoPriority.High, dueDate: DateTime.Today.AddDays(-2))
        };

        var result = _handler.Handle(todos, listId: 1);

        Assert.NotNull(result);
        Assert.Contains("overdue", result!.Reason, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("high priority", result.Reason, StringComparison.OrdinalIgnoreCase);
    }
}
