using TodoApp.Features.Todos;
using TodoApp.Features.Todos.Export;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.Tags;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Export;

public class IcsExportHandlerTests
{
    private static readonly IcsExportHandler Handler = new();
    private static readonly DateTime FixedNow = new(2026, 7, 25, 12, 0, 0, DateTimeKind.Utc);

    private static TodoSummary MakeTodo(
        int id = 1,
        string title = "Test",
        bool isCompleted = false,
        DateTime? dueDate = null,
        string? notes = null) =>
        new(id, title, isCompleted, new DateTime(2026, 1, 1, 10, 0, 0), TodoPriority.None, dueDate, Notes: notes);

    [Fact]
    public void Generate_EmptyList_ReturnsCalendarWithNoEvents()
    {
        var ics = Handler.Generate([], nowUtc: FixedNow);

        Assert.Contains("BEGIN:VCALENDAR", ics);
        Assert.Contains("END:VCALENDAR", ics);
        Assert.DoesNotContain("BEGIN:VEVENT", ics);
    }

    [Fact]
    public void Generate_TodoWithDueDate_CreatesEvent()
    {
        var todo = MakeTodo(title: "Pay rent", dueDate: new DateTime(2026, 8, 1));

        var ics = Handler.Generate([todo], nowUtc: FixedNow);

        Assert.Contains("BEGIN:VEVENT", ics);
        Assert.Contains("SUMMARY:Pay rent", ics);
        Assert.Contains("DTSTART;VALUE=DATE:20260801", ics);
        Assert.Contains("END:VEVENT", ics);
    }

    [Fact]
    public void Generate_TodoWithoutDueDate_IsExcluded()
    {
        var todo = MakeTodo(title: "No due date", dueDate: null);

        var ics = Handler.Generate([todo], nowUtc: FixedNow);

        Assert.DoesNotContain("BEGIN:VEVENT", ics);
    }

    [Fact]
    public void Generate_CompletedTodo_HasCompletedStatus()
    {
        var todo = MakeTodo(isCompleted: true, dueDate: new DateTime(2026, 8, 1));

        var ics = Handler.Generate([todo], nowUtc: FixedNow);

        Assert.Contains("STATUS:COMPLETED", ics);
    }

    [Fact]
    public void Generate_ActiveTodo_HasConfirmedStatus()
    {
        var todo = MakeTodo(isCompleted: false, dueDate: new DateTime(2026, 8, 1));

        var ics = Handler.Generate([todo], nowUtc: FixedNow);

        Assert.Contains("STATUS:CONFIRMED", ics);
    }

    [Fact]
    public void Generate_TitleWithSpecialCharacters_IsEscaped()
    {
        var todo = MakeTodo(title: "Buy milk; eggs, bread", dueDate: new DateTime(2026, 8, 1));

        var ics = Handler.Generate([todo], nowUtc: FixedNow);

        Assert.Contains("SUMMARY:Buy milk\\; eggs\\, bread", ics);
    }

    [Fact]
    public void Generate_WithNotes_IncludesDescription()
    {
        var todo = MakeTodo(dueDate: new DateTime(2026, 8, 1), notes: "Remember the receipt");

        var ics = Handler.Generate([todo], nowUtc: FixedNow);

        Assert.Contains("DESCRIPTION:Remember the receipt", ics);
    }

    [Fact]
    public void Generate_WithoutNotes_HasNoDescriptionLine()
    {
        var todo = MakeTodo(dueDate: new DateTime(2026, 8, 1), notes: null);

        var ics = Handler.Generate([todo], nowUtc: FixedNow);

        Assert.DoesNotContain("DESCRIPTION:", ics);
    }

    [Fact]
    public void Generate_WithTags_IncludesCategories()
    {
        var todo = MakeTodo(id: 1, dueDate: new DateTime(2026, 8, 1));
        var tags = new Dictionary<int, List<Tag>>
        {
            [1] = [new Tag(1, 1, "finance"), new Tag(2, 1, "urgent")]
        };

        var ics = Handler.Generate([todo], tags, nowUtc: FixedNow);

        Assert.Contains("CATEGORIES:finance,urgent", ics);
    }

    [Fact]
    public void Generate_WithoutTags_HasNoCategoriesLine()
    {
        var todo = MakeTodo(id: 1, dueDate: new DateTime(2026, 8, 1));

        var ics = Handler.Generate([todo], nowUtc: FixedNow);

        Assert.DoesNotContain("CATEGORIES:", ics);
    }

    [Fact]
    public void Generate_MultipleTodosWithDueDates_AllAppearAsEvents()
    {
        var todos = new[]
        {
            MakeTodo(id: 1, title: "First", dueDate: new DateTime(2026, 8, 1)),
            MakeTodo(id: 2, title: "Second", dueDate: new DateTime(2026, 8, 2)),
            MakeTodo(id: 3, title: "Third", dueDate: null),
        };

        var ics = Handler.Generate(todos, nowUtc: FixedNow);

        Assert.Contains("SUMMARY:First", ics);
        Assert.Contains("SUMMARY:Second", ics);
        Assert.DoesNotContain("SUMMARY:Third", ics);
        Assert.Equal(2, ics.Split("BEGIN:VEVENT").Length - 1);
    }

    [Fact]
    public void Generate_EachEventHasUniqueUid()
    {
        var todos = new[]
        {
            MakeTodo(id: 5, dueDate: new DateTime(2026, 8, 1)),
            MakeTodo(id: 9, dueDate: new DateTime(2026, 8, 2)),
        };

        var ics = Handler.Generate(todos, nowUtc: FixedNow);

        Assert.Contains("UID:todo-5@todoapp.local", ics);
        Assert.Contains("UID:todo-9@todoapp.local", ics);
    }
}
