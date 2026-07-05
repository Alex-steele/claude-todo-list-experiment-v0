using TodoApp.Features.Todos;
using TodoApp.Features.Todos.ColorLabel;
using TodoApp.Features.Todos.Export;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.RecurringTodos;
using TodoApp.Features.Todos.Tags;
using TodoApp.Features.Todos.TimeEstimates;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Export;

public class MarkdownExportHandlerTests
{
    private static readonly MarkdownExportHandler Handler = new();

    private static TodoSummary MakeTodo(
        int id = 1,
        string title = "Test todo",
        bool isCompleted = false,
        TodoPriority priority = TodoPriority.None,
        DateTime? dueDate = null,
        bool isPinned = false,
        DateTime? completedAt = null) =>
        new(id, title, isCompleted, new DateTime(2026, 1, 1), priority, dueDate,
            isPinned, null, RecurrenceRule.None, 1, completedAt,
            TimeEstimate.None, TodoColorLabel.None);

    [Fact]
    public void Generate_IncludesListNameInHeading()
    {
        var md = Handler.Generate("Work", [], null);

        Assert.Contains("# Work", md);
    }

    [Fact]
    public void Generate_IncludesExportDate()
    {
        var md = Handler.Generate("Tasks", [], null);

        Assert.Contains(DateTime.Today.ToString("yyyy-MM-dd"), md);
    }

    [Fact]
    public void Generate_ActiveTodo_FormattedWithEmptyCheckbox()
    {
        var todos = new[] { MakeTodo(1, "Buy milk") };

        var md = Handler.Generate("Personal", todos, null);

        Assert.Contains("- [ ] Buy milk", md);
    }

    [Fact]
    public void Generate_CompletedTodo_FormattedWithCheckedBoxAndStrikethrough()
    {
        var todos = new[] { MakeTodo(1, "Done task", isCompleted: true) };

        var md = Handler.Generate("Personal", todos, null);

        Assert.Contains("- [x] ~~Done task~~", md);
    }

    [Fact]
    public void Generate_ActiveSection_BeforeCompletedSection()
    {
        var todos = new[]
        {
            MakeTodo(1, "Active", isCompleted: false),
            MakeTodo(2, "Done", isCompleted: true),
        };

        var md = Handler.Generate("Tasks", todos, null);

        var activeIdx = md.IndexOf("## Active", StringComparison.Ordinal);
        var completedIdx = md.IndexOf("## Completed", StringComparison.Ordinal);
        Assert.True(activeIdx < completedIdx);
    }

    [Fact]
    public void Generate_NoActiveTodos_OmitsActiveSection()
    {
        var todos = new[] { MakeTodo(1, "Done", isCompleted: true) };

        var md = Handler.Generate("Tasks", todos, null);

        Assert.DoesNotContain("## Active", md);
        Assert.Contains("## Completed", md);
    }

    [Fact]
    public void Generate_NoCompletedTodos_OmitsCompletedSection()
    {
        var todos = new[] { MakeTodo(1, "Active") };

        var md = Handler.Generate("Tasks", todos, null);

        Assert.Contains("## Active", md);
        Assert.DoesNotContain("## Completed", md);
    }

    [Fact]
    public void Generate_PriorityIncludedInMeta()
    {
        var todos = new[] { MakeTodo(1, "Urgent task", priority: TodoPriority.High) };

        var md = Handler.Generate("Tasks", todos, null);

        Assert.Contains("high", md);
    }

    [Fact]
    public void Generate_DueDateTodayShownAsDueToday()
    {
        var todos = new[] { MakeTodo(1, "Today task", dueDate: DateTime.Today) };

        var md = Handler.Generate("Tasks", todos, null);

        Assert.Contains("due today", md);
    }

    [Fact]
    public void Generate_OverdueDateMarkedAsOverdue()
    {
        var past = DateTime.Today.AddDays(-3);
        var todos = new[] { MakeTodo(1, "Late task", dueDate: past) };

        var md = Handler.Generate("Tasks", todos, null);

        Assert.Contains("overdue", md);
    }

    [Fact]
    public void Generate_FutureDueDateIncludesDate()
    {
        var future = DateTime.Today.AddDays(7);
        var todos = new[] { MakeTodo(1, "Future task", dueDate: future) };

        var md = Handler.Generate("Tasks", todos, null);

        Assert.Contains($"due {future:yyyy-MM-dd}", md);
    }

    [Fact]
    public void Generate_TagsIncludedWithHashPrefix()
    {
        var todos = new[] { MakeTodo(1, "Work task") };
        var tags = new Dictionary<int, List<Tag>>
        {
            [1] = [new Tag(1, 1, "work"), new Tag(2, 1, "urgent")]
        };

        var md = Handler.Generate("Tasks", todos, tags);

        Assert.Contains("#work", md);
        Assert.Contains("#urgent", md);
    }

    [Fact]
    public void Generate_PinnedTodoHasPinEmoji()
    {
        var todos = new[] { MakeTodo(1, "Pinned task", isPinned: true) };

        var md = Handler.Generate("Tasks", todos, null);

        Assert.Contains("📌", md);
    }

    [Fact]
    public void Generate_ActiveTodosOrderedByPriorityHighFirst()
    {
        var todos = new[]
        {
            MakeTodo(1, "Low task",    priority: TodoPriority.Low),
            MakeTodo(2, "High task",   priority: TodoPriority.High),
            MakeTodo(3, "Medium task", priority: TodoPriority.Medium),
        };

        var md = Handler.Generate("Tasks", todos, null);

        var highIdx   = md.IndexOf("High task",   StringComparison.Ordinal);
        var mediumIdx = md.IndexOf("Medium task", StringComparison.Ordinal);
        var lowIdx    = md.IndexOf("Low task",    StringComparison.Ordinal);
        Assert.True(highIdx < mediumIdx);
        Assert.True(mediumIdx < lowIdx);
    }

    [Fact]
    public void Generate_NoPriorityTodo_NoMetaAppended()
    {
        var todos = new[] { MakeTodo(1, "Simple task") };

        var md = Handler.Generate("Tasks", todos, null);

        Assert.Contains("- [ ] Simple task\n", md);
    }

    [Fact]
    public void Generate_EmptyTodoList_ReturnsHeaderOnly()
    {
        var md = Handler.Generate("Empty", [], null);

        Assert.Contains("# Empty", md);
        Assert.DoesNotContain("## Active", md);
        Assert.DoesNotContain("## Completed", md);
    }
}
