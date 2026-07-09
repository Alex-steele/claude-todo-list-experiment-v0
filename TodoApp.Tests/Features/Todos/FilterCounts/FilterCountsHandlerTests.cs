using TodoApp.Features.Todos;
using TodoApp.Features.Todos.FilterCounts;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.Tags;
using Xunit;

namespace TodoApp.Tests.Features.Todos.FilterCounts;

public class FilterCountsHandlerTests
{
    private readonly FilterCountsHandler _handler = new();

    private static TodoSummary MakeTodo(int id, TodoPriority priority = TodoPriority.None, bool completed = false) =>
        new(id, $"Todo {id}", completed, DateTime.UtcNow, priority, null);

    private static Dictionary<int, List<Tag>> MakeTags(params (int TodoId, string[] Names)[] entries)
    {
        return entries.ToDictionary(
            e => e.TodoId,
            e => e.Names.Select((n, i) => new Tag(i + 1, e.TodoId, n)).ToList());
    }

    [Fact]
    public void Handle_EmptyList_ReturnsEmptyCounts()
    {
        var result = _handler.Handle([], new Dictionary<int, List<Tag>>());
        Assert.Empty(result.ByPriority);
        Assert.Empty(result.ByTag);
    }

    [Fact]
    public void Handle_ActiveTodosByPriority_CountedCorrectly()
    {
        var todos = new[]
        {
            MakeTodo(1, TodoPriority.High),
            MakeTodo(2, TodoPriority.High),
            MakeTodo(3, TodoPriority.Medium),
            MakeTodo(4, TodoPriority.None)
        };
        var result = _handler.Handle(todos, new Dictionary<int, List<Tag>>());
        Assert.Equal(2, result.ByPriority[TodoPriority.High]);
        Assert.Equal(1, result.ByPriority[TodoPriority.Medium]);
        Assert.Equal(1, result.ByPriority[TodoPriority.None]);
        Assert.False(result.ByPriority.ContainsKey(TodoPriority.Low));
    }

    [Fact]
    public void Handle_CompletedTodos_ExcludedFromCounts()
    {
        var todos = new[]
        {
            MakeTodo(1, TodoPriority.High),
            MakeTodo(2, TodoPriority.High, completed: true)
        };
        var result = _handler.Handle(todos, new Dictionary<int, List<Tag>>());
        Assert.Equal(1, result.ByPriority[TodoPriority.High]);
    }

    [Fact]
    public void Handle_TagCounts_CountedCorrectly()
    {
        var todos = new[] { MakeTodo(1), MakeTodo(2), MakeTodo(3) };
        var tags = MakeTags(
            (1, ["work", "urgent"]),
            (2, ["work"]),
            (3, ["home"]));

        var result = _handler.Handle(todos, tags);
        Assert.Equal(2, result.ByTag["work"]);
        Assert.Equal(1, result.ByTag["urgent"]);
        Assert.Equal(1, result.ByTag["home"]);
    }

    [Fact]
    public void Handle_TagsOnCompletedTodos_Excluded()
    {
        var todos = new[] { MakeTodo(1), MakeTodo(2, completed: true) };
        var tags = MakeTags(
            (1, ["work"]),
            (2, ["work"]));

        var result = _handler.Handle(todos, tags);
        Assert.Equal(1, result.ByTag["work"]);
    }

    [Fact]
    public void Handle_TodoWithNoTags_CountsAsPriorityOnly()
    {
        var todos = new[] { MakeTodo(1, TodoPriority.Low) };
        var result = _handler.Handle(todos, new Dictionary<int, List<Tag>>());
        Assert.Equal(1, result.ByPriority[TodoPriority.Low]);
        Assert.Empty(result.ByTag);
    }

    [Fact]
    public void Handle_TagCounts_CaseInsensitiveAggregation()
    {
        var todos = new[] { MakeTodo(1), MakeTodo(2) };
        var tags = MakeTags(
            (1, ["Work"]),
            (2, ["work"]));

        var result = _handler.Handle(todos, tags);
        // Should aggregate case-insensitively
        Assert.Single(result.ByTag);
        Assert.Equal(2, result.ByTag.Values.First());
    }

    [Fact]
    public void Handle_MissingPriorityKey_ReturnsZeroViaDefault()
    {
        var todos = new[] { MakeTodo(1, TodoPriority.High) };
        var result = _handler.Handle(todos, new Dictionary<int, List<Tag>>());
        Assert.Equal(0, result.ByPriority.GetValueOrDefault(TodoPriority.Low));
    }

    [Fact]
    public void Handle_NoDueDate_CountsActiveTodosWithoutDueDate()
    {
        var todos = new[]
        {
            MakeTodo(1),                               // no due date
            MakeTodo(2),                               // no due date
            new TodoSummary(3, "With date", false, DateTime.UtcNow, TodoPriority.None, DateTime.Today)
        };
        var result = _handler.Handle(todos, new Dictionary<int, List<Tag>>());
        Assert.Equal(2, result.NoDueDate);
    }

    [Fact]
    public void Handle_NoDueDate_ExcludesCompleted()
    {
        var todos = new[]
        {
            MakeTodo(1),                                               // active, no date
            MakeTodo(2, completed: true),                              // completed, no date
        };
        var result = _handler.Handle(todos, new Dictionary<int, List<Tag>>());
        Assert.Equal(1, result.NoDueDate);
    }

    [Fact]
    public void Handle_NoDueDate_ZeroWhenAllHaveDates()
    {
        var todos = new[]
        {
            new TodoSummary(1, "A", false, DateTime.UtcNow, TodoPriority.None, DateTime.Today),
            new TodoSummary(2, "B", false, DateTime.UtcNow, TodoPriority.None, DateTime.Today.AddDays(1)),
        };
        var result = _handler.Handle(todos, new Dictionary<int, List<Tag>>());
        Assert.Equal(0, result.NoDueDate);
    }

    [Fact]
    public void Handle_NoDueDate_ZeroWhenEmpty()
    {
        var result = _handler.Handle([], new Dictionary<int, List<Tag>>());
        Assert.Equal(0, result.NoDueDate);
    }
}
