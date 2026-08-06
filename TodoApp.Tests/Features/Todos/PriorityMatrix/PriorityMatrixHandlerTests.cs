using TodoApp.Features.Todos;
using TodoApp.Features.Todos.ColorLabel;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.PriorityMatrix;
using TodoApp.Features.Todos.RecurringTodos;
using TodoApp.Features.Todos.TimeEstimates;
using Xunit;

namespace TodoApp.Tests.Features.Todos.PriorityMatrix;

public class PriorityMatrixHandlerTests
{
    private readonly PriorityMatrixHandler _handler = new();

    private static TodoSummary MakeTodo(
        int id,
        TodoPriority priority = TodoPriority.None,
        DateTime? dueDate = null,
        bool isCompleted = false,
        int listId = 1) =>
        new(id, $"Todo {id}", isCompleted, DateTime.Today.AddDays(-10), priority, dueDate,
            false, null, RecurrenceRule.None, listId, null, TimeEstimate.None, TodoColorLabel.None);

    [Fact]
    public void Handle_NoTodos_ReturnsEmptyQuadrantsAndNoData()
    {
        var result = _handler.Handle([], listId: 1);

        Assert.False(result.HasData);
        Assert.Empty(result.UrgentImportant);
        Assert.Empty(result.ImportantNotUrgent);
        Assert.Empty(result.UrgentNotImportant);
        Assert.Empty(result.NeitherUrgentNorImportant);
    }

    [Fact]
    public void Handle_HighPriorityOverdue_GoesInUrgentImportant()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, priority: TodoPriority.High, dueDate: DateTime.Today.AddDays(-1))
        };

        var result = _handler.Handle(todos, listId: 1);

        Assert.True(result.HasData);
        Assert.Equal(1, result.UrgentImportant.Single().Id);
    }

    [Fact]
    public void Handle_HighPriorityDueToday_GoesInUrgentImportant()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, priority: TodoPriority.High, dueDate: DateTime.Today)
        };

        var result = _handler.Handle(todos, listId: 1);

        Assert.Equal(1, result.UrgentImportant.Single().Id);
    }

    [Fact]
    public void Handle_HighPriorityNoDueDate_GoesInImportantNotUrgent()
    {
        var todos = new List<TodoSummary> { MakeTodo(1, priority: TodoPriority.High, dueDate: null) };

        var result = _handler.Handle(todos, listId: 1);

        Assert.Equal(1, result.ImportantNotUrgent.Single().Id);
    }

    [Fact]
    public void Handle_HighPriorityDueInFuture_GoesInImportantNotUrgent()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, priority: TodoPriority.High, dueDate: DateTime.Today.AddDays(5))
        };

        var result = _handler.Handle(todos, listId: 1);

        Assert.Equal(1, result.ImportantNotUrgent.Single().Id);
    }

    [Theory]
    [InlineData(TodoPriority.Medium)]
    [InlineData(TodoPriority.Low)]
    [InlineData(TodoPriority.None)]
    public void Handle_NonHighPriorityOverdue_GoesInUrgentNotImportant(TodoPriority priority)
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, priority: priority, dueDate: DateTime.Today.AddDays(-1))
        };

        var result = _handler.Handle(todos, listId: 1);

        Assert.Equal(1, result.UrgentNotImportant.Single().Id);
    }

    [Fact]
    public void Handle_NonHighPriorityNoDueDate_GoesInNeitherUrgentNorImportant()
    {
        var todos = new List<TodoSummary> { MakeTodo(1, priority: TodoPriority.Medium, dueDate: null) };

        var result = _handler.Handle(todos, listId: 1);

        Assert.Equal(1, result.NeitherUrgentNorImportant.Single().Id);
    }

    [Fact]
    public void Handle_NonHighPriorityDueInFuture_GoesInNeitherUrgentNorImportant()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, priority: TodoPriority.Low, dueDate: DateTime.Today.AddDays(3))
        };

        var result = _handler.Handle(todos, listId: 1);

        Assert.Equal(1, result.NeitherUrgentNorImportant.Single().Id);
    }

    [Fact]
    public void Handle_CompletedTodos_ExcludedFromAllQuadrants()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, priority: TodoPriority.High, dueDate: DateTime.Today.AddDays(-1), isCompleted: true)
        };

        var result = _handler.Handle(todos, listId: 1);

        Assert.False(result.HasData);
    }

    [Fact]
    public void Handle_TodosFromOtherLists_ExcludedFromAllQuadrants()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, priority: TodoPriority.High, dueDate: DateTime.Today.AddDays(-1), listId: 2)
        };

        var result = _handler.Handle(todos, listId: 1);

        Assert.False(result.HasData);
    }

    [Fact]
    public void Handle_MixOfTodos_SortsEachIntoCorrectQuadrant()
    {
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, priority: TodoPriority.High, dueDate: DateTime.Today.AddDays(-1)),
            MakeTodo(2, priority: TodoPriority.High, dueDate: null),
            MakeTodo(3, priority: TodoPriority.Low, dueDate: DateTime.Today),
            MakeTodo(4, priority: TodoPriority.None, dueDate: DateTime.Today.AddDays(10))
        };

        var result = _handler.Handle(todos, listId: 1);

        Assert.True(result.HasData);
        Assert.Equal(1, result.UrgentImportant.Single().Id);
        Assert.Equal(2, result.ImportantNotUrgent.Single().Id);
        Assert.Equal(3, result.UrgentNotImportant.Single().Id);
        Assert.Equal(4, result.NeitherUrgentNorImportant.Single().Id);
    }
}
