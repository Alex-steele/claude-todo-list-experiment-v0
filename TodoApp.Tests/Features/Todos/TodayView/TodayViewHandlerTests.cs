using TodoApp.Features.Lists;
using TodoApp.Features.Todos;
using TodoApp.Features.Todos.ColorLabel;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.RecurringTodos;
using TodoApp.Features.Todos.TimeEstimates;
using TodoApp.Features.Todos.TodayView;
using Xunit;

namespace TodoApp.Tests.Features.Todos.TodayView;

public class TodayViewHandlerTests
{
    private static readonly DateTime Today = DateTime.Today;
    private static readonly IReadOnlyList<TodoList> DefaultLists =
    [
        new(1, "Personal"),
        new(2, "Work"),
    ];

    private static TodoSummary MakeTodo(int id, string title, DateTime? dueDate, bool isCompleted = false,
        int listId = 1, TodoPriority priority = TodoPriority.None) =>
        new(id, title, isCompleted, DateTime.UtcNow, priority, dueDate,
            ListId: listId);

    private readonly TodayViewHandler _handler = new();

    [Fact]
    public void Returns_Empty_When_No_Todos_Are_Due()
    {
        var todos = new[]
        {
            MakeTodo(1, "Future task", Today.AddDays(3)),
            MakeTodo(2, "No date task", null),
        };

        var result = _handler.Handle(todos, DefaultLists);

        Assert.Empty(result);
    }

    [Fact]
    public void Returns_Overdue_And_DueToday_Todos()
    {
        var todos = new[]
        {
            MakeTodo(1, "Overdue task", Today.AddDays(-2)),
            MakeTodo(2, "Due today task", Today),
            MakeTodo(3, "Future task", Today.AddDays(1)),
        };

        var result = _handler.Handle(todos, DefaultLists);

        var group = Assert.Single(result);
        Assert.Equal(2, group.Todos.Count);
        Assert.Contains(group.Todos, t => t.Title == "Overdue task");
        Assert.Contains(group.Todos, t => t.Title == "Due today task");
    }

    [Fact]
    public void Excludes_Completed_Todos()
    {
        var todos = new[]
        {
            MakeTodo(1, "Overdue but done", Today.AddDays(-1), isCompleted: true),
            MakeTodo(2, "Active overdue", Today.AddDays(-1)),
        };

        var result = _handler.Handle(todos, DefaultLists);

        var group = Assert.Single(result);
        var todo = Assert.Single(group.Todos);
        Assert.Equal("Active overdue", todo.Title);
    }

    [Fact]
    public void Groups_Todos_By_List()
    {
        var todos = new[]
        {
            MakeTodo(1, "Personal overdue", Today.AddDays(-1), listId: 1),
            MakeTodo(2, "Work overdue", Today.AddDays(-1), listId: 2),
            MakeTodo(3, "Another personal", Today, listId: 1),
        };

        var result = _handler.Handle(todos, DefaultLists);

        Assert.Equal(2, result.Count);
        var personalGroup = result.First(g => g.ListId == 1);
        Assert.Equal("Personal", personalGroup.ListName);
        Assert.Equal(2, personalGroup.Todos.Count);

        var workGroup = result.First(g => g.ListId == 2);
        Assert.Equal("Work", workGroup.ListName);
        Assert.Single(workGroup.Todos);
    }

    [Fact]
    public void Sorts_Groups_By_Count_Descending()
    {
        var todos = new[]
        {
            MakeTodo(1, "Personal 1", Today.AddDays(-2), listId: 1),
            MakeTodo(2, "Personal 2", Today, listId: 1),
            MakeTodo(3, "Work 1", Today, listId: 2),
            MakeTodo(4, "Personal 3", Today.AddDays(-1), listId: 1),
        };

        var result = _handler.Handle(todos, DefaultLists);

        Assert.Equal(1, result[0].ListId);
        Assert.Equal(3, result[0].Todos.Count);
        Assert.Equal(2, result[1].ListId);
    }

    [Fact]
    public void Sorts_Todos_Within_Group_OldestOverdueFirst_ThenDueToday()
    {
        var todos = new[]
        {
            MakeTodo(1, "Due today", Today, listId: 1),
            MakeTodo(2, "Overdue 1 day ago", Today.AddDays(-1), listId: 1),
            MakeTodo(3, "Overdue 3 days ago", Today.AddDays(-3), listId: 1),
        };

        var result = _handler.Handle(todos, DefaultLists);

        var group = Assert.Single(result);
        Assert.Equal("Overdue 3 days ago", group.Todos[0].Title);
        Assert.Equal("Overdue 1 day ago", group.Todos[1].Title);
        Assert.Equal("Due today", group.Todos[2].Title);
    }

    [Fact]
    public void Sorts_By_Priority_Within_Same_DueDate()
    {
        var todos = new[]
        {
            MakeTodo(1, "Low priority", Today.AddDays(-1), priority: TodoPriority.Low),
            MakeTodo(2, "High priority", Today.AddDays(-1), priority: TodoPriority.High),
            MakeTodo(3, "Medium priority", Today.AddDays(-1), priority: TodoPriority.Medium),
        };

        var result = _handler.Handle(todos, DefaultLists);

        var group = Assert.Single(result);
        Assert.Equal("High priority", group.Todos[0].Title);
        Assert.Equal("Medium priority", group.Todos[1].Title);
        Assert.Equal("Low priority", group.Todos[2].Title);
    }

    [Fact]
    public void CountUrgent_Returns_Count_Of_Active_Urgent_Todos()
    {
        var todos = new[]
        {
            MakeTodo(1, "Overdue", Today.AddDays(-1)),
            MakeTodo(2, "Due today", Today),
            MakeTodo(3, "Future", Today.AddDays(1)),
            MakeTodo(4, "Overdue but done", Today.AddDays(-2), isCompleted: true),
        };

        var count = _handler.CountUrgent(todos);

        Assert.Equal(2, count);
    }

    [Fact]
    public void CountUrgent_Returns_Zero_When_Nothing_Urgent()
    {
        var todos = new[]
        {
            MakeTodo(1, "Future", Today.AddDays(5)),
            MakeTodo(2, "No date", null),
        };

        var count = _handler.CountUrgent(todos);

        Assert.Equal(0, count);
    }

    [Fact]
    public void Uses_List_Name_Unknown_When_ListId_Not_In_Lists()
    {
        var todos = new[]
        {
            MakeTodo(1, "Orphan todo", Today.AddDays(-1), listId: 99),
        };

        var result = _handler.Handle(todos, DefaultLists);

        var group = Assert.Single(result);
        Assert.Equal("List 99", group.ListName);
    }

    [Fact]
    public void Returns_Empty_When_All_Todos_Completed()
    {
        var todos = new[]
        {
            MakeTodo(1, "Done overdue", Today.AddDays(-1), isCompleted: true),
            MakeTodo(2, "Done today", Today, isCompleted: true),
        };

        var result = _handler.Handle(todos, DefaultLists);

        Assert.Empty(result);
    }
}
