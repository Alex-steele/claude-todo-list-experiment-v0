using TodoApp.Features.Todos;
using TodoApp.Features.Todos.FilterSortTodos;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.TimeEstimates;
using Xunit;

namespace TodoApp.Tests.Features.Todos.FilterSortTodos;

public class FilterSortTodosHandlerTests
{
    private static readonly DateTime Base = new(2026, 1, 1);

    private static TodoSummary Make(int id, string title,
        bool isCompleted = false,
        TodoPriority priority = TodoPriority.None,
        DateTime? dueDate = null,
        bool isPinned = false,
        TimeEstimate timeEstimate = TimeEstimate.None)
        => new(id, title, isCompleted, Base.AddSeconds(id), priority, dueDate, isPinned, TimeEstimate: timeEstimate);

    private readonly FilterSortTodosHandler _handler = new();

    [Fact]
    public void PriorityFilter_Null_ReturnsAllTodos()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "High task", priority: TodoPriority.High),
            Make(2, "Low task",  priority: TodoPriority.Low),
            Make(3, "None task")
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest, priorityFilter: null);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void PriorityFilter_High_ReturnsOnlyHighPriority()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "High task",   priority: TodoPriority.High),
            Make(2, "Medium task", priority: TodoPriority.Medium),
            Make(3, "Low task",    priority: TodoPriority.Low),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            priorityFilter: TodoPriority.High);

        Assert.Single(result);
        Assert.Equal("High task", result[0].Title);
    }

    [Fact]
    public void PriorityFilter_None_ReturnsOnlyNoPriorityTodos()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "No priority"),
            Make(2, "High task", priority: TodoPriority.High),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            priorityFilter: TodoPriority.None);

        Assert.Single(result);
        Assert.Equal("No priority", result[0].Title);
    }

    [Fact]
    public void PriorityFilter_CombinesWithStatusFilter()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "Active high",    priority: TodoPriority.High),
            Make(2, "Completed high", priority: TodoPriority.High, isCompleted: true),
            Make(3, "Active medium",  priority: TodoPriority.Medium),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.Active, TodoSortOrder.Newest,
            priorityFilter: TodoPriority.High);

        Assert.Single(result);
        Assert.Equal("Active high", result[0].Title);
    }

    [Fact]
    public void PriorityFilter_CombinesWithSearch()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "Buy milk",   priority: TodoPriority.High),
            Make(2, "Buy coffee", priority: TodoPriority.Low),
            Make(3, "Walk dog",   priority: TodoPriority.High),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            searchQuery: "buy", priorityFilter: TodoPriority.High);

        Assert.Single(result);
        Assert.Equal("Buy milk", result[0].Title);
    }

    [Fact]
    public void PriorityFilter_NoMatch_ReturnsEmpty()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "Low task", priority: TodoPriority.Low),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            priorityFilter: TodoPriority.High);

        Assert.Empty(result);
    }

    // ── Date filter ───────────────────────────────────────────────────────────

    [Fact]
    public void DateFilter_None_ReturnsAll()
    {
        var today = DateTime.Today;
        var todos = new List<TodoSummary>
        {
            Make(1, "Overdue",   dueDate: today.AddDays(-1)),
            Make(2, "Today",     dueDate: today),
            Make(3, "Future",    dueDate: today.AddDays(3)),
            Make(4, "No due date")
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            dateFilter: TodoDateFilter.None);

        Assert.Equal(4, result.Count);
    }

    [Fact]
    public void DateFilter_Overdue_ReturnsOnlyOverdueTodos()
    {
        var today = DateTime.Today;
        var todos = new List<TodoSummary>
        {
            Make(1, "Overdue",    dueDate: today.AddDays(-2)),
            Make(2, "Also overdue", dueDate: today.AddDays(-1)),
            Make(3, "Due today",  dueDate: today),
            Make(4, "Future",     dueDate: today.AddDays(3)),
            Make(5, "No due date")
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            dateFilter: TodoDateFilter.Overdue);

        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.True(t.DueDate!.Value.Date < today));
    }

    [Fact]
    public void DateFilter_Overdue_ExcludesCompletedTodos()
    {
        var today = DateTime.Today;
        var todos = new List<TodoSummary>
        {
            Make(1, "Overdue active",    dueDate: today.AddDays(-1)),
            Make(2, "Overdue completed", dueDate: today.AddDays(-1), isCompleted: true),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            dateFilter: TodoDateFilter.Overdue);

        Assert.Single(result);
        Assert.Equal("Overdue active", result[0].Title);
    }

    [Fact]
    public void DateFilter_DueToday_ReturnsOnlyTodayTodos()
    {
        var today = DateTime.Today;
        var todos = new List<TodoSummary>
        {
            Make(1, "Overdue",   dueDate: today.AddDays(-1)),
            Make(2, "Today",     dueDate: today),
            Make(3, "Tomorrow",  dueDate: today.AddDays(1)),
            Make(4, "No due date")
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            dateFilter: TodoDateFilter.DueToday);

        Assert.Single(result);
        Assert.Equal("Today", result[0].Title);
    }

    [Fact]
    public void DateFilter_DueThisWeek_ReturnsNextSevenDays()
    {
        var today = DateTime.Today;
        var todos = new List<TodoSummary>
        {
            Make(1, "Overdue",       dueDate: today.AddDays(-1)),
            Make(2, "Today",         dueDate: today),
            Make(3, "In 3 days",     dueDate: today.AddDays(3)),
            Make(4, "In 6 days",     dueDate: today.AddDays(6)),
            Make(5, "In 7 days",     dueDate: today.AddDays(7)),  // exclusive
            Make(6, "No due date")
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            dateFilter: TodoDateFilter.DueThisWeek);

        Assert.Equal(3, result.Count); // Today, +3, +6
        Assert.DoesNotContain(result, t => t.Title == "Overdue");
        Assert.DoesNotContain(result, t => t.Title == "In 7 days");
    }

    [Fact]
    public void DateFilter_DueThisWeek_ExcludesCompletedTodos()
    {
        var today = DateTime.Today;
        var todos = new List<TodoSummary>
        {
            Make(1, "Active",    dueDate: today.AddDays(2)),
            Make(2, "Completed", dueDate: today.AddDays(2), isCompleted: true),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            dateFilter: TodoDateFilter.DueThisWeek);

        Assert.Single(result);
        Assert.Equal("Active", result[0].Title);
    }

    [Fact]
    public void TimeEstimateFilter_Any_ReturnsAllTodos()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "No estimate"),
            Make(2, "15 min task",  timeEstimate: TimeEstimate.FifteenMinutes),
            Make(3, "1 hour task",  timeEstimate: TimeEstimate.OneHour),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            timeEstimateFilter: TodoTimeEstimateFilter.Any);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void TimeEstimateFilter_NoEstimate_ReturnsOnlyTodosWithNoEstimate()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "No estimate"),
            Make(2, "15 min task",  timeEstimate: TimeEstimate.FifteenMinutes),
            Make(3, "1 hour task",  timeEstimate: TimeEstimate.OneHour),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            timeEstimateFilter: TodoTimeEstimateFilter.NoEstimate);

        Assert.Single(result);
        Assert.Equal("No estimate", result[0].Title);
    }

    [Fact]
    public void TimeEstimateFilter_Max15Min_ReturnsOnlyFifteenMinuteTodos()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "Quick task",   timeEstimate: TimeEstimate.FifteenMinutes),
            Make(2, "Medium task",  timeEstimate: TimeEstimate.ThirtyMinutes),
            Make(3, "Long task",    timeEstimate: TimeEstimate.OneHour),
            Make(4, "No estimate"),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            timeEstimateFilter: TodoTimeEstimateFilter.Max15Min);

        Assert.Single(result);
        Assert.Equal("Quick task", result[0].Title);
    }

    [Fact]
    public void TimeEstimateFilter_Max30Min_IncludesFifteenAndThirtyMinute()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "15 min task",  timeEstimate: TimeEstimate.FifteenMinutes),
            Make(2, "30 min task",  timeEstimate: TimeEstimate.ThirtyMinutes),
            Make(3, "1 hour task",  timeEstimate: TimeEstimate.OneHour),
            Make(4, "No estimate"),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            timeEstimateFilter: TodoTimeEstimateFilter.Max30Min);

        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.NotEqual("No estimate", t.Title));
        Assert.All(result, t => Assert.NotEqual("1 hour task", t.Title));
    }

    [Fact]
    public void TimeEstimateFilter_Max1Hour_IncludesUpToOneHour()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "15 min task",  timeEstimate: TimeEstimate.FifteenMinutes),
            Make(2, "30 min task",  timeEstimate: TimeEstimate.ThirtyMinutes),
            Make(3, "1 hour task",  timeEstimate: TimeEstimate.OneHour),
            Make(4, "2 hour task",  timeEstimate: TimeEstimate.TwoHours),
            Make(5, "No estimate"),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            timeEstimateFilter: TodoTimeEstimateFilter.Max1Hour);

        Assert.Equal(3, result.Count);
        Assert.DoesNotContain(result, t => t.Title == "2 hour task");
        Assert.DoesNotContain(result, t => t.Title == "No estimate");
    }

    [Fact]
    public void TimeEstimateFilter_Max2Hours_ExcludesLongerEstimatesAndNoEstimate()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "30 min task",  timeEstimate: TimeEstimate.ThirtyMinutes),
            Make(2, "2 hour task",  timeEstimate: TimeEstimate.TwoHours),
            Make(3, "4 hour task",  timeEstimate: TimeEstimate.FourHours),
            Make(4, "All day task", timeEstimate: TimeEstimate.OneDay),
            Make(5, "No estimate"),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            timeEstimateFilter: TodoTimeEstimateFilter.Max2Hours);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Title == "30 min task");
        Assert.Contains(result, t => t.Title == "2 hour task");
    }

    [Fact]
    public void TimeEstimateFilter_CombinesWithStatusFilter()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "Active quick",     timeEstimate: TimeEstimate.FifteenMinutes),
            Make(2, "Completed quick",  timeEstimate: TimeEstimate.FifteenMinutes, isCompleted: true),
            Make(3, "Active slow",      timeEstimate: TimeEstimate.OneDay),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.Active, TodoSortOrder.Newest,
            timeEstimateFilter: TodoTimeEstimateFilter.Max15Min);

        Assert.Single(result);
        Assert.Equal("Active quick", result[0].Title);
    }
}
