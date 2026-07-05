using TodoApp.Features.Todos;
using TodoApp.Features.Todos.ColorLabel;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.RecurringTodos;
using TodoApp.Features.Todos.TagStats;
using TodoApp.Features.Todos.Tags;
using TodoApp.Features.Todos.TimeEstimates;
using Xunit;

namespace TodoApp.Tests.Features.Todos.TagStats;

public class TagStatsHandlerTests
{
    private readonly TagStatsHandler _handler = new();

    private static TodoSummary MakeTodo(int id, bool isCompleted = false) =>
        new(id, $"Todo {id}", isCompleted, DateTime.UtcNow, TodoPriority.None, null,
            false, null, RecurrenceRule.None, 1, isCompleted ? DateTime.UtcNow : null,
            TimeEstimate.None, TodoColorLabel.None);

    private static Dictionary<int, List<Tag>> Tags(params (int todoId, string[] names)[] entries)
    {
        var dict = new Dictionary<int, List<Tag>>();
        var tagId = 1;
        foreach (var (todoId, names) in entries)
            dict[todoId] = names.Select(n => new Tag(tagId++, todoId, n)).ToList();
        return dict;
    }

    [Fact]
    public void Returns_Empty_When_NoTodos()
    {
        var result = _handler.Handle([], []);

        Assert.Empty(result);
    }

    [Fact]
    public void Returns_Empty_When_NoTags()
    {
        var todos = new[] { MakeTodo(1), MakeTodo(2, isCompleted: true) };

        var result = _handler.Handle(todos, []);

        Assert.Empty(result);
    }

    [Fact]
    public void Counts_Active_And_Completed_Per_Tag()
    {
        var todos = new[] { MakeTodo(1), MakeTodo(2, isCompleted: true) };
        var tags = Tags((1, ["work"]), (2, ["work"]));

        var result = _handler.Handle(todos, tags);

        var stat = Assert.Single(result);
        Assert.Equal("work", stat.TagName);
        Assert.Equal(1, stat.Active);
        Assert.Equal(1, stat.Completed);
        Assert.Equal(2, stat.Total);
        Assert.Equal(50, stat.CompletionPercent);
    }

    [Fact]
    public void Handles_Multiple_Tags_On_Same_Todo()
    {
        var todos = new[] { MakeTodo(1) };
        var tags = Tags((1, ["work", "urgent"]));

        var result = _handler.Handle(todos, tags);

        Assert.Equal(2, result.Count);
        Assert.All(result, s => { Assert.Equal(1, s.Active); Assert.Equal(0, s.Completed); });
    }

    [Fact]
    public void Handles_Different_Tags_On_Different_Todos()
    {
        var todos = new[] { MakeTodo(1), MakeTodo(2, isCompleted: true) };
        var tags = Tags((1, ["personal"]), (2, ["work"]));

        var result = _handler.Handle(todos, tags);

        Assert.Equal(2, result.Count);
        var personal = result.First(s => s.TagName == "personal");
        var work = result.First(s => s.TagName == "work");
        Assert.Equal(1, personal.Active);
        Assert.Equal(0, personal.Completed);
        Assert.Equal(0, work.Active);
        Assert.Equal(1, work.Completed);
    }

    [Fact]
    public void Orders_By_CompletionPercent_Ascending_Lowest_First()
    {
        // "urgent" has 0%, "work" has 100%, "personal" has 50%
        var todos = new[] { MakeTodo(1), MakeTodo(2, isCompleted: true), MakeTodo(3), MakeTodo(4, isCompleted: true) };
        var tags = Tags(
            (1, ["urgent"]),
            (2, ["work"]),
            (3, ["personal"]),
            (4, ["personal"]));

        var result = _handler.Handle(todos, tags);

        Assert.Equal("urgent",   result[0].TagName);  // 0%
        Assert.Equal("personal", result[1].TagName);  // 50%
        Assert.Equal("work",     result[2].TagName);  // 100%
    }

    [Fact]
    public void TiesInCompletionPercent_OrderedAlphabetically()
    {
        var todos = new[] { MakeTodo(1), MakeTodo(2) };
        var tags = Tags((1, ["zebra"]), (2, ["apple"]));

        var result = _handler.Handle(todos, tags);

        Assert.Equal("apple",  result[0].TagName);
        Assert.Equal("zebra",  result[1].TagName);
    }

    [Fact]
    public void Ignores_TodoIds_Not_In_Todos_List()
    {
        var todos = new[] { MakeTodo(1) };
        var tags = Tags((99, ["ghost"]));  // todoId 99 not in todos

        var result = _handler.Handle(todos, tags);

        Assert.Empty(result);
    }

    [Fact]
    public void CompletionPercent_Zero_When_AllActive()
    {
        var todos = new[] { MakeTodo(1), MakeTodo(2) };
        var tags = Tags((1, ["x"]), (2, ["x"]));

        var result = _handler.Handle(todos, tags);

        var stat = Assert.Single(result);
        Assert.Equal(0, stat.CompletionPercent);
    }

    [Fact]
    public void CompletionPercent_100_When_AllCompleted()
    {
        var todos = new[] { MakeTodo(1, isCompleted: true), MakeTodo(2, isCompleted: true) };
        var tags = Tags((1, ["done"]), (2, ["done"]));

        var result = _handler.Handle(todos, tags);

        var stat = Assert.Single(result);
        Assert.Equal(100, stat.CompletionPercent);
    }

    [Fact]
    public void TagName_Comparison_Is_CaseInsensitive()
    {
        // "Work" and "work" should merge into one stat
        var todos = new[] { MakeTodo(1), MakeTodo(2, isCompleted: true) };
        var tags = Tags((1, ["Work"]), (2, ["work"]));

        var result = _handler.Handle(todos, tags);

        var stat = Assert.Single(result);
        Assert.Equal(1, stat.Active);
        Assert.Equal(1, stat.Completed);
    }
}
