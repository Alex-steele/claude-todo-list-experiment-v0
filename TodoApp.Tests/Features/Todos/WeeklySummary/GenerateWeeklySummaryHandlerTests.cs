using TodoApp.Features.Todos;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.WeeklySummary;
using Xunit;

namespace TodoApp.Tests.Features.Todos.WeeklySummary;

public class GenerateWeeklySummaryHandlerTests
{
    private static readonly DateTime Now = new(2026, 6, 29, 12, 0, 0, DateTimeKind.Utc);
    private readonly GenerateWeeklySummaryHandler _handler = new();

    private static TodoSummary Completed(int id, string title, DateTime completedAt)
        => new(id, title, IsCompleted: true, CreatedAt: completedAt.AddDays(-1),
               Priority: TodoPriority.None, DueDate: null, CompletedAt: completedAt);

    private static TodoSummary Active(int id, string title)
        => new(id, title, IsCompleted: false, CreatedAt: Now.AddDays(-1),
               Priority: TodoPriority.None, DueDate: null);

    [Fact]
    public void Handle_NoTodos_ReturnsNoneMessage()
    {
        var result = _handler.Handle([], Now);

        Assert.Contains("No todos completed", result);
    }

    [Fact]
    public void Handle_NoCompletedInWindow_ReturnsNoneMessage()
    {
        var todos = new List<TodoSummary>
        {
            Completed(1, "Old task", Now.AddDays(-8)),
        }.AsReadOnly();

        var result = _handler.Handle(todos, Now);

        Assert.Contains("No todos completed", result);
    }

    [Fact]
    public void Handle_OnlyActiveTodos_ReturnsNoneMessage()
    {
        var todos = new List<TodoSummary>
        {
            Active(1, "Pending task"),
        }.AsReadOnly();

        var result = _handler.Handle(todos, Now);

        Assert.Contains("No todos completed", result);
    }

    [Fact]
    public void Handle_CompletedWithinWeek_IncludesTitles()
    {
        var todos = new List<TodoSummary>
        {
            Completed(1, "Fix bug",     Now.AddDays(-1)),
            Completed(2, "Write tests", Now.AddDays(-3)),
        }.AsReadOnly();

        var result = _handler.Handle(todos, Now);

        Assert.Contains("Fix bug", result);
        Assert.Contains("Write tests", result);
    }

    [Fact]
    public void Handle_IncludesCountInHeader()
    {
        var todos = new List<TodoSummary>
        {
            Completed(1, "Task A", Now.AddDays(-1)),
            Completed(2, "Task B", Now.AddDays(-2)),
            Completed(3, "Task C", Now.AddDays(-3)),
        }.AsReadOnly();

        var result = _handler.Handle(todos, Now);

        Assert.Contains("(3)", result);
    }

    [Fact]
    public void Handle_ExcludesTodosCompletedExactlySevenDaysAgo()
    {
        var todos = new List<TodoSummary>
        {
            Completed(1, "Boundary task", Now.AddDays(-7).AddSeconds(-1)),
        }.AsReadOnly();

        var result = _handler.Handle(todos, Now);

        Assert.DoesNotContain("Boundary task", result);
    }

    [Fact]
    public void Handle_IncludesTodosCompletedJustInsideWindow()
    {
        var todos = new List<TodoSummary>
        {
            Completed(1, "Just inside", Now.AddDays(-7).AddSeconds(1)),
        }.AsReadOnly();

        var result = _handler.Handle(todos, Now);

        Assert.Contains("Just inside", result);
    }

    [Fact]
    public void Handle_ExcludesActiveTodos()
    {
        var todos = new List<TodoSummary>
        {
            Completed(1, "Done task",   Now.AddDays(-1)),
            Active(2,    "Active task"),
        }.AsReadOnly();

        var result = _handler.Handle(todos, Now);

        Assert.Contains("Done task", result);
        Assert.DoesNotContain("Active task", result);
    }

    [Fact]
    public void Handle_UsesBulletPointFormat()
    {
        var todos = new List<TodoSummary>
        {
            Completed(1, "My task", Now.AddDays(-1)),
        }.AsReadOnly();

        var result = _handler.Handle(todos, Now);

        Assert.Contains("• My task", result);
    }

    [Fact]
    public void Handle_DefaultsToCurrentUtcTime_WhenReferenceNotProvided()
    {
        var handler = new GenerateWeeklySummaryHandler();
        var recentlyCompleted = Completed(1, "Recent", DateTime.UtcNow.AddHours(-1));

        var result = handler.Handle([recentlyCompleted]);

        Assert.Contains("Recent", result);
    }
}
