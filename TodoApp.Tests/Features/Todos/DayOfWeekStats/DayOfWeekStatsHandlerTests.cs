using TodoApp.Features.Todos;
using TodoApp.Features.Todos.ColorLabel;
using TodoApp.Features.Todos.DayOfWeekStats;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.RecurringTodos;
using TodoApp.Features.Todos.TimeEstimates;
using Xunit;

namespace TodoApp.Tests.Features.Todos.DayOfWeekStats;

public class DayOfWeekStatsHandlerTests
{
    private readonly DayOfWeekStatsHandler _handler = new();

    private static TodoSummary MakeTodo(
        int id,
        bool isCompleted,
        DateTime? completedAt = null,
        int listId = 1) =>
        new(id, $"Todo {id}", isCompleted, DateTime.Today.AddDays(-10), TodoPriority.None, null,
            false, null, RecurrenceRule.None, listId, completedAt, TimeEstimate.None, TodoColorLabel.None);

    [Fact]
    public void Handle_NoTodos_ReturnsAllZeroCountsAndNoBestDay()
    {
        var result = _handler.Handle([], listId: 1);

        Assert.False(result.HasData);
        Assert.Null(result.BestDay);
        Assert.Equal(7, result.Counts.Count);
        Assert.All(result.Counts, c => Assert.Equal(0, c.Count));
    }

    [Fact]
    public void Handle_OnlyActiveTodos_ExcludedFromCounts()
    {
        var todos = new List<TodoSummary> { MakeTodo(1, isCompleted: false) };

        var result = _handler.Handle(todos, listId: 1);

        Assert.False(result.HasData);
        Assert.Null(result.BestDay);
    }

    [Fact]
    public void Handle_CompletedTodoWithNoCompletedAt_ExcludedFromCounts()
    {
        var todos = new List<TodoSummary> { MakeTodo(1, isCompleted: true, completedAt: null) };

        var result = _handler.Handle(todos, listId: 1);

        Assert.False(result.HasData);
    }

    [Fact]
    public void Handle_TodosFromOtherLists_ExcludedFromCounts()
    {
        // A Tuesday
        var completedAt = new DateTime(2026, 7, 21);
        var todos = new List<TodoSummary> { MakeTodo(1, isCompleted: true, completedAt: completedAt, listId: 2) };

        var result = _handler.Handle(todos, listId: 1);

        Assert.False(result.HasData);
    }

    [Fact]
    public void Handle_SingleCompletion_CountsOnCorrectDay()
    {
        // 2026-07-21 is a Tuesday
        var completedAt = new DateTime(2026, 7, 21);
        var todos = new List<TodoSummary> { MakeTodo(1, isCompleted: true, completedAt: completedAt) };

        var result = _handler.Handle(todos, listId: 1);

        Assert.True(result.HasData);
        Assert.Equal(1, result.Counts.Single(c => c.Day == DayOfWeek.Tuesday).Count);
        Assert.All(result.Counts.Where(c => c.Day != DayOfWeek.Tuesday), c => Assert.Equal(0, c.Count));
        Assert.Equal(DayOfWeek.Tuesday, result.BestDay);
    }

    [Fact]
    public void Handle_MultipleCompletionsAcrossDays_BestDayIsHighestCount()
    {
        var tuesday = new DateTime(2026, 7, 21);
        var wednesday = new DateTime(2026, 7, 22);
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, isCompleted: true, completedAt: tuesday),
            MakeTodo(2, isCompleted: true, completedAt: wednesday),
            MakeTodo(3, isCompleted: true, completedAt: wednesday),
        };

        var result = _handler.Handle(todos, listId: 1);

        Assert.Equal(1, result.Counts.Single(c => c.Day == DayOfWeek.Tuesday).Count);
        Assert.Equal(2, result.Counts.Single(c => c.Day == DayOfWeek.Wednesday).Count);
        Assert.Equal(DayOfWeek.Wednesday, result.BestDay);
    }

    [Fact]
    public void Handle_TiedCounts_BestDayIsEarliestDayOfWeek()
    {
        // Sunday and Wednesday tied at 1 each — Sunday (index 0) should win the tie.
        var sunday = new DateTime(2026, 7, 19);
        var wednesday = new DateTime(2026, 7, 22);
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, isCompleted: true, completedAt: sunday),
            MakeTodo(2, isCompleted: true, completedAt: wednesday),
        };

        var result = _handler.Handle(todos, listId: 1);

        Assert.Equal(DayOfWeek.Sunday, result.BestDay);
    }

    [Fact]
    public void Handle_AlwaysReturnsAllSevenDays()
    {
        var result = _handler.Handle([], listId: 1);

        var days = result.Counts.Select(c => c.Day).ToHashSet();
        Assert.Equal(7, days.Count);
        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
            Assert.Contains(day, days);
    }
}
