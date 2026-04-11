using TodoApp.Features.Todos;
using TodoApp.Features.Todos.Export;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.Tags;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Export;

public class CsvExportHandlerTests
{
    private static readonly CsvExportHandler Handler = new();

    private static TodoSummary MakeTodo(
        int id = 1,
        string title = "Test",
        bool isCompleted = false,
        TodoPriority priority = TodoPriority.None,
        DateTime? dueDate = null) =>
        new(id, title, isCompleted, new DateTime(2026, 1, 1, 10, 0, 0), priority, dueDate);

    [Fact]
    public void Generate_EmptyList_ReturnsHeaderOnly()
    {
        var csv = Handler.Generate([]);

        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Single(lines);
        Assert.Contains("Id,Title,Priority,DueDate,IsCompleted,CreatedAt,Tags", lines[0]);
    }

    [Fact]
    public void Generate_SingleTodo_AppearsAfterHeader()
    {
        var todo = MakeTodo(id: 1, title: "Buy milk");

        var csv = Handler.Generate([todo]);

        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, lines.Length);
        Assert.Contains("Buy milk", lines[1]);
        Assert.Contains("1,", lines[1]);
    }

    [Fact]
    public void Generate_TitleWithComma_IsQuoted()
    {
        var todo = MakeTodo(title: "Eggs, milk, bread");

        var csv = Handler.Generate([todo]);

        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Contains("\"Eggs, milk, bread\"", lines[1]);
    }

    [Fact]
    public void Generate_TitleWithDoubleQuote_IsEscaped()
    {
        var todo = MakeTodo(title: "Buy \"premium\" milk");

        var csv = Handler.Generate([todo]);

        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Contains("\"Buy \"\"premium\"\" milk\"", lines[1]);
    }

    [Fact]
    public void Generate_TodoWithDueDate_IncludesFormattedDate()
    {
        var todo = MakeTodo(dueDate: new DateTime(2026, 4, 15));

        var csv = Handler.Generate([todo]);

        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Contains("2026-04-15", lines[1]);
    }

    [Fact]
    public void Generate_TodoWithoutDueDate_HasEmptyDateColumn()
    {
        var todo = MakeTodo(dueDate: null);

        var csv = Handler.Generate([todo]);

        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        // DueDate column should be empty (two consecutive commas around it)
        Assert.Contains("None,,False", lines[1]);
    }

    [Fact]
    public void Generate_CompletedTodo_ShowsTrue()
    {
        var todo = MakeTodo(isCompleted: true);

        var csv = Handler.Generate([todo]);

        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Contains("True", lines[1]);
    }

    [Fact]
    public void Generate_WithTags_IncludesPipeSeparatedTagNames()
    {
        var todo = MakeTodo(id: 1);
        var tags = new Dictionary<int, List<Tag>>
        {
            [1] = [new Tag(1, 1, "work"), new Tag(2, 1, "urgent")]
        };

        var csv = Handler.Generate([todo], tags);

        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Contains("work|urgent", lines[1]);
    }

    [Fact]
    public void Generate_WithoutTags_HasEmptyTagsColumn()
    {
        var todo = MakeTodo(id: 1);

        var csv = Handler.Generate([todo]);

        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        // Last column (Tags) should be empty
        Assert.EndsWith(",", lines[1]);
    }

    [Fact]
    public void Generate_MultipleTodos_AllAppear()
    {
        var todos = new[]
        {
            MakeTodo(id: 1, title: "First"),
            MakeTodo(id: 2, title: "Second"),
            MakeTodo(id: 3, title: "Third"),
        };

        var csv = Handler.Generate(todos);

        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(4, lines.Length); // header + 3 todos
        Assert.Contains("First", lines[1]);
        Assert.Contains("Second", lines[2]);
        Assert.Contains("Third", lines[3]);
    }
}
