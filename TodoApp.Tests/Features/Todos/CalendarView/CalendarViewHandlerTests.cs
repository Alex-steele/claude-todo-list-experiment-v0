using TodoApp.Features.Todos;
using TodoApp.Features.Todos.CalendarView;
using TodoApp.Features.Todos.GetTodos;
using Xunit;

namespace TodoApp.Tests.Features.Todos.CalendarView;

public class CalendarViewHandlerTests
{
    private static TodoSummary MakeTodo(int id, string title, DateTime? dueDate, TodoPriority priority = TodoPriority.None, bool isCompleted = false, int listId = 1)
        => new(id, title, isCompleted, DateTime.UtcNow, priority, dueDate, ListId: listId);

    [Fact]
    public void Handle_ReturnsOneCellPerDayInMonth()
    {
        var handler = new CalendarViewHandler();

        var days = handler.Handle([], listId: 1, year: 2026, month: 2);

        Assert.Equal(28, days.Count); // Feb 2026 is not a leap year
        Assert.Equal(new DateTime(2026, 2, 1), days.First().Date);
        Assert.Equal(new DateTime(2026, 2, 28), days.Last().Date);
        Assert.All(days, d => Assert.Empty(d.Todos));
    }

    [Fact]
    public void Handle_PlacesTodoOnItsDueDateCell()
    {
        var handler = new CalendarViewHandler();
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, "Pay rent", new DateTime(2026, 7, 15)),
        };

        var days = handler.Handle(todos, listId: 1, year: 2026, month: 7);

        var cell = days.Single(d => d.Date == new DateTime(2026, 7, 15));
        Assert.Single(cell.Todos);
        Assert.Equal("Pay rent", cell.Todos[0].Title);
        Assert.All(days.Where(d => d.Date != new DateTime(2026, 7, 15)), d => Assert.Empty(d.Todos));
    }

    [Fact]
    public void Handle_ExcludesTodosOutsideRequestedMonth()
    {
        var handler = new CalendarViewHandler();
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, "Last month", new DateTime(2026, 6, 30)),
            MakeTodo(2, "Next month", new DateTime(2026, 8, 1)),
            MakeTodo(3, "No due date", null),
        };

        var days = handler.Handle(todos, listId: 1, year: 2026, month: 7);

        Assert.All(days, d => Assert.Empty(d.Todos));
    }

    [Fact]
    public void Handle_ExcludesTodosFromOtherLists()
    {
        var handler = new CalendarViewHandler();
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, "Other list todo", new DateTime(2026, 7, 10), listId: 2),
        };

        var days = handler.Handle(todos, listId: 1, year: 2026, month: 7);

        Assert.All(days, d => Assert.Empty(d.Todos));
    }

    [Fact]
    public void Handle_GroupsMultipleTodosOnSameDay_SortedByCompletionThenPriority()
    {
        var handler = new CalendarViewHandler();
        var date = new DateTime(2026, 7, 4);
        var todos = new List<TodoSummary>
        {
            MakeTodo(1, "Done low", date, TodoPriority.Low, isCompleted: true),
            MakeTodo(2, "Active high", date, TodoPriority.High),
            MakeTodo(3, "Active medium", date, TodoPriority.Medium),
        };

        var days = handler.Handle(todos, listId: 1, year: 2026, month: 7);

        var cell = days.Single(d => d.Date == date);
        Assert.Equal(3, cell.Todos.Count);
        Assert.Equal(["Active high", "Active medium", "Done low"], cell.Todos.Select(t => t.Title));
    }

    [Fact]
    public void Handle_LeapYearFebruary_Returns29Days()
    {
        var handler = new CalendarViewHandler();

        var days = handler.Handle([], listId: 1, year: 2028, month: 2);

        Assert.Equal(29, days.Count);
    }
}
