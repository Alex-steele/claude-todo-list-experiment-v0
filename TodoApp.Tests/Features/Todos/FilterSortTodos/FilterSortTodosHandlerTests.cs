using TodoApp.Features.Todos;
using TodoApp.Features.Todos.FilterSortTodos;
using TodoApp.Features.Todos.GetTodos;
using Xunit;

namespace TodoApp.Tests.Features.Todos.FilterSortTodos;

public class FilterSortTodosHandlerTests
{
    private static readonly DateTime Base = new(2026, 1, 1);

    private static TodoSummary Make(int id, string title,
        bool isCompleted = false,
        TodoPriority priority = TodoPriority.None,
        DateTime? dueDate = null,
        bool isPinned = false)
        => new(id, title, isCompleted, Base.AddSeconds(id), priority, dueDate, isPinned);

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
}
