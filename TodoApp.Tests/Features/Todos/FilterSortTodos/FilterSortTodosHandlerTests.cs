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
        TimeEstimate timeEstimate = TimeEstimate.None,
        string? notes = null)
        => new(id, title, isCompleted, Base.AddSeconds(id), priority, dueDate, isPinned, notes, TimeEstimate: timeEstimate);

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

    // ── Alphabetical sort ─────────────────────────────────────────────────────

    [Fact]
    public void TitleAsc_SortsAlphabeticallyAscending()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "Zebra"),
            Make(2, "Apple"),
            Make(3, "Mango"),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.TitleAsc);

        Assert.Equal(["Apple", "Mango", "Zebra"], result.Select(t => t.Title).ToArray());
    }

    [Fact]
    public void TitleDesc_SortsAlphabeticallyDescending()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "Apple"),
            Make(2, "Zebra"),
            Make(3, "Mango"),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.TitleDesc);

        Assert.Equal(["Zebra", "Mango", "Apple"], result.Select(t => t.Title).ToArray());
    }

    [Fact]
    public void TitleAsc_IsCaseInsensitive()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "banana"),
            Make(2, "Apple"),
            Make(3, "cherry"),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.TitleAsc);

        Assert.Equal("Apple", result[0].Title);
        Assert.Equal("banana", result[1].Title);
        Assert.Equal("cherry", result[2].Title);
    }

    [Fact]
    public void TitleAsc_PinnedTodosStillAppearFirst()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "Alpha"),
            Make(2, "Pinned task", isPinned: true),
            Make(3, "Beta"),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.TitleAsc);

        Assert.Equal("Pinned task", result[0].Title);
        Assert.Equal(["Alpha", "Beta"], result.Skip(1).Select(t => t.Title).ToArray());
    }

    [Fact]
    public void TitleDesc_PinnedTodosStillAppearFirst()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "Alpha"),
            Make(2, "Pinned task", isPinned: true),
            Make(3, "Zeta"),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.TitleDesc);

        Assert.Equal("Pinned task", result[0].Title);
        Assert.Equal(["Zeta", "Alpha"], result.Skip(1).Select(t => t.Title).ToArray());
    }

    [Fact]
    public void TitleAsc_CombinesWithStatusFilter()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "Carrot"),
            Make(2, "Apple",  isCompleted: true),
            Make(3, "Banana"),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.Active, TodoSortOrder.TitleAsc);

        Assert.Equal(["Banana", "Carrot"], result.Select(t => t.Title).ToArray());
    }

    // ── Notes search ──────────────────────────────────────────────────────────

    [Fact]
    public void Search_MatchesNotes_WhenTitleDoesNotMatch()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "Buy groceries", notes: "milk and eggs"),
            Make(2, "Walk the dog"),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest, searchQuery: "milk");

        Assert.Single(result);
        Assert.Equal("Buy groceries", result[0].Title);
    }

    [Fact]
    public void Search_NotesMatch_IsCaseInsensitive()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "Prepare meeting", notes: "Contact Alice about the agenda"),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest, searchQuery: "alice");

        Assert.Single(result);
    }

    [Fact]
    public void Search_NullNotes_DoesNotIncludeNotesMismatch()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "Walk the dog", notes: null),
            Make(2, "Buy groceries", notes: "eggs and cheese"),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest, searchQuery: "milk");

        Assert.Empty(result);
    }

    [Fact]
    public void Search_ReturnsOnce_WhenBothTitleAndNotesMatch()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "Buy milk", notes: "get full-fat milk from the store"),
            Make(2, "Walk the dog"),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest, searchQuery: "milk");

        Assert.Single(result);
        Assert.Equal("Buy milk", result[0].Title);
    }

    [Fact]
    public void Search_Notes_CombinesWithStatusFilter()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "Prepare report", notes: "include revenue figures", isCompleted: false),
            Make(2, "Old report",     notes: "include revenue figures", isCompleted: true),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.Active, TodoSortOrder.Newest, searchQuery: "revenue");

        Assert.Single(result);
        Assert.Equal("Prepare report", result[0].Title);
    }

    [Fact]
    public void Search_Notes_CombinesWithPriorityFilter()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "High task",   notes: "check the dashboard", priority: TodoPriority.High),
            Make(2, "Low task",    notes: "check the dashboard", priority: TodoPriority.Low),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            searchQuery: "dashboard", priorityFilter: TodoPriority.High);

        Assert.Single(result);
        Assert.Equal("High task", result[0].Title);
    }

    [Fact]
    public void Sort_TimeEstimateAsc_OrdersShortestFirst()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "Long",   timeEstimate: TimeEstimate.FourHours),
            Make(2, "Short",  timeEstimate: TimeEstimate.FifteenMinutes),
            Make(3, "Medium", timeEstimate: TimeEstimate.OneHour),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.TimeEstimateAsc);

        Assert.Equal("Short",  result[0].Title);
        Assert.Equal("Medium", result[1].Title);
        Assert.Equal("Long",   result[2].Title);
    }

    [Fact]
    public void Sort_TimeEstimateDesc_OrdersLongestFirst()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "Short",  timeEstimate: TimeEstimate.FifteenMinutes),
            Make(2, "Long",   timeEstimate: TimeEstimate.FourHours),
            Make(3, "Medium", timeEstimate: TimeEstimate.TwoHours),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.TimeEstimateDesc);

        Assert.Equal("Long",   result[0].Title);
        Assert.Equal("Medium", result[1].Title);
        Assert.Equal("Short",  result[2].Title);
    }

    [Fact]
    public void Sort_TimeEstimateAsc_NoneEstimateGoesToEnd()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "No estimate"),
            Make(2, "Quick",  timeEstimate: TimeEstimate.FifteenMinutes),
            Make(3, "Medium", timeEstimate: TimeEstimate.OneHour),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.TimeEstimateAsc);

        Assert.Equal("Quick",       result[0].Title);
        Assert.Equal("Medium",      result[1].Title);
        Assert.Equal("No estimate", result[2].Title);
    }

    [Fact]
    public void Sort_TimeEstimateDesc_NoneEstimateGoesToEnd()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "No estimate"),
            Make(2, "Quick",  timeEstimate: TimeEstimate.FifteenMinutes),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.TimeEstimateDesc);

        Assert.Equal("Quick",       result[0].Title);
        Assert.Equal("No estimate", result[1].Title);
    }

    [Fact]
    public void Sort_TimeEstimateAsc_PinnedStillSortsFirst()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "Unpinned short",  timeEstimate: TimeEstimate.FifteenMinutes),
            Make(2, "Pinned long",     timeEstimate: TimeEstimate.OneDay, isPinned: true),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.TimeEstimateAsc);

        Assert.Equal("Pinned long",    result[0].Title);
        Assert.Equal("Unpinned short", result[1].Title);
    }

    [Fact]
    public void Sort_TimeEstimateAsc_CombinesWithStatusFilter()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "Active long",      isCompleted: false, timeEstimate: TimeEstimate.TwoHours),
            Make(2, "Completed short",  isCompleted: true,  timeEstimate: TimeEstimate.FifteenMinutes),
            Make(3, "Active short",     isCompleted: false, timeEstimate: TimeEstimate.ThirtyMinutes),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.Active, TodoSortOrder.TimeEstimateAsc);

        Assert.Equal(2, result.Count);
        Assert.Equal("Active short", result[0].Title);
        Assert.Equal("Active long",  result[1].Title);
    }

    private static TodoSummary MakeAged(int id, string title, int daysOld, bool isCompleted = false)
        => new(id, title, isCompleted, DateTime.Today.AddDays(-daysOld), TodoPriority.None, null);

    [Fact]
    public void StalenessFilter_Any_ReturnsAllTodos()
    {
        var todos = new List<TodoSummary>
        {
            MakeAged(1, "New today",     daysOld: 0),
            MakeAged(2, "One week old",  daysOld: 7),
            MakeAged(3, "One month old", daysOld: 30),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            stalenessFilter: TodoStalenessFilter.Any);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void StalenessFilter_OneWeekPlus_ExcludesRecentTodos()
    {
        var todos = new List<TodoSummary>
        {
            MakeAged(1, "New today",   daysOld: 0),
            MakeAged(2, "Three days",  daysOld: 3),
            MakeAged(3, "Six days",    daysOld: 6),
            MakeAged(4, "Seven days",  daysOld: 7),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            stalenessFilter: TodoStalenessFilter.OneWeekPlus);

        Assert.Single(result);
        Assert.Equal("Seven days", result[0].Title);
    }

    [Fact]
    public void StalenessFilter_OneWeekPlus_ExcludesCompletedTodos()
    {
        var todos = new List<TodoSummary>
        {
            MakeAged(1, "Active old",    daysOld: 14),
            MakeAged(2, "Completed old", daysOld: 14, isCompleted: true),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            stalenessFilter: TodoStalenessFilter.OneWeekPlus);

        Assert.Single(result);
        Assert.Equal("Active old", result[0].Title);
    }

    [Fact]
    public void StalenessFilter_TwoWeeksPlus_ExcludesOneWeekOldTodos()
    {
        var todos = new List<TodoSummary>
        {
            MakeAged(1, "Eight days",    daysOld: 8),
            MakeAged(2, "Fourteen days", daysOld: 14),
            MakeAged(3, "Thirty days",   daysOld: 30),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            stalenessFilter: TodoStalenessFilter.TwoWeeksPlus);

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, t => t.Title == "Eight days");
    }

    [Fact]
    public void StalenessFilter_OneMonthPlus_ExcludesTwoWeekOldTodos()
    {
        var todos = new List<TodoSummary>
        {
            MakeAged(1, "Two weeks",   daysOld: 14),
            MakeAged(2, "Thirty days", daysOld: 30),
            MakeAged(3, "Two months",  daysOld: 60),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            stalenessFilter: TodoStalenessFilter.OneMonthPlus);

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, t => t.Title == "Two weeks");
    }

    [Fact]
    public void StalenessFilter_CombinesWithStatusFilter()
    {
        var todos = new List<TodoSummary>
        {
            MakeAged(1, "Active old",      daysOld: 14),
            MakeAged(2, "Completed old",   daysOld: 14, isCompleted: true),
            MakeAged(3, "Active new",      daysOld: 2),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.Active, TodoSortOrder.Newest,
            stalenessFilter: TodoStalenessFilter.OneWeekPlus);

        Assert.Single(result);
        Assert.Equal("Active old", result[0].Title);
    }

    // --- Recommended sort ---

    [Fact]
    public void Sort_Recommended_OverdueTodosBeforeDueToday()
    {
        var today = DateTime.Today;
        var todos = new List<TodoSummary>
        {
            Make(1, "Due today",  dueDate: today),
            Make(2, "Overdue",    dueDate: today.AddDays(-1)),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Recommended);

        Assert.Equal("Overdue",   result[0].Title);
        Assert.Equal("Due today", result[1].Title);
    }

    [Fact]
    public void Sort_Recommended_DueTodayBeforeDueThisWeek()
    {
        var today = DateTime.Today;
        var todos = new List<TodoSummary>
        {
            Make(1, "This week",  dueDate: today.AddDays(3)),
            Make(2, "Due today",  dueDate: today),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Recommended);

        Assert.Equal("Due today",  result[0].Title);
        Assert.Equal("This week",  result[1].Title);
    }

    [Fact]
    public void Sort_Recommended_DueThisWeekBeforeNoDueDate()
    {
        var today = DateTime.Today;
        var todos = new List<TodoSummary>
        {
            Make(1, "No date",    dueDate: null),
            Make(2, "This week",  dueDate: today.AddDays(4)),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Recommended);

        Assert.Equal("This week", result[0].Title);
        Assert.Equal("No date",   result[1].Title);
    }

    [Fact]
    public void Sort_Recommended_WithinOverdueMostOverdueFirst()
    {
        var today = DateTime.Today;
        var todos = new List<TodoSummary>
        {
            Make(1, "Yesterday",   dueDate: today.AddDays(-1)),
            Make(2, "Last week",   dueDate: today.AddDays(-7)),
            Make(3, "Two weeks ago", dueDate: today.AddDays(-14)),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Recommended);

        Assert.Equal("Two weeks ago", result[0].Title);
        Assert.Equal("Last week",     result[1].Title);
        Assert.Equal("Yesterday",     result[2].Title);
    }

    [Fact]
    public void Sort_Recommended_WithinSameBucketHighPriorityFirst()
    {
        var today = DateTime.Today;
        var todos = new List<TodoSummary>
        {
            Make(1, "Low today",    dueDate: today, priority: TodoPriority.Low),
            Make(2, "High today",   dueDate: today, priority: TodoPriority.High),
            Make(3, "Medium today", dueDate: today, priority: TodoPriority.Medium),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Recommended);

        Assert.Equal("High today",   result[0].Title);
        Assert.Equal("Medium today", result[1].Title);
        Assert.Equal("Low today",    result[2].Title);
    }

    [Fact]
    public void Sort_Recommended_PinnedAlwaysFirst()
    {
        var today = DateTime.Today;
        var todos = new List<TodoSummary>
        {
            Make(1, "Overdue",  dueDate: today.AddDays(-3)),
            Make(2, "Pinned no date", isPinned: true),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Recommended);

        Assert.Equal("Pinned no date", result[0].Title);
        Assert.Equal("Overdue",        result[1].Title);
    }

    [Fact]
    public void DateFilter_NoDueDate_ReturnsOnlyTodosWithoutDueDate()
    {
        var today = DateTime.Today;
        var todos = new List<TodoSummary>
        {
            Make(1, "No date"),
            Make(2, "Has date", dueDate: today.AddDays(3)),
            Make(3, "Also no date"),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            dateFilter: TodoDateFilter.NoDueDate);

        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Null(t.DueDate));
    }

    [Fact]
    public void DateFilter_NoDueDate_ExcludesCompletedTodos()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "Active no date"),
            Make(2, "Completed no date", isCompleted: true),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            dateFilter: TodoDateFilter.NoDueDate);

        Assert.Single(result);
        Assert.Equal("Active no date", result[0].Title);
    }

    [Fact]
    public void DateFilter_NoDueDate_EmptyWhenAllTodosHaveDates()
    {
        var today = DateTime.Today;
        var todos = new List<TodoSummary>
        {
            Make(1, "Due tomorrow", dueDate: today.AddDays(1)),
            Make(2, "Due next week", dueDate: today.AddDays(7)),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            dateFilter: TodoDateFilter.NoDueDate);

        Assert.Empty(result);
    }

    [Fact]
    public void DateFilter_NoDueDate_CombinesWithPriorityFilter()
    {
        var todos = new List<TodoSummary>
        {
            Make(1, "High no date",    priority: TodoPriority.High),
            Make(2, "Low no date",     priority: TodoPriority.Low),
            Make(3, "High with date",  priority: TodoPriority.High, dueDate: DateTime.Today.AddDays(1)),
        }.AsReadOnly();

        var result = _handler.Handle(todos, TodoStatusFilter.All, TodoSortOrder.Newest,
            priorityFilter: TodoPriority.High, dateFilter: TodoDateFilter.NoDueDate);

        Assert.Single(result);
        Assert.Equal("High no date", result[0].Title);
    }
}
