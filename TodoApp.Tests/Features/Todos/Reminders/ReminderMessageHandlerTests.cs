using TodoApp.Features.Todos.DueSummary;
using TodoApp.Features.Todos.Reminders;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Reminders;

public class ReminderMessageHandlerTests
{
    private readonly ReminderMessageHandler _handler = new();

    [Fact]
    public void Handle_NoOverdueOrDueToday_ReturnsNull()
    {
        var result = _handler.Handle(new DueSummaryResult(0, 0));

        Assert.Null(result);
    }

    [Fact]
    public void Handle_OnlyOverdue_MentionsOverdueOnly()
    {
        var result = _handler.Handle(new DueSummaryResult(Overdue: 2, DueToday: 0));

        Assert.Equal("You have 2 overdue.", result);
    }

    [Fact]
    public void Handle_OnlyDueToday_MentionsDueTodayOnly()
    {
        var result = _handler.Handle(new DueSummaryResult(Overdue: 0, DueToday: 3));

        Assert.Equal("You have 3 due today.", result);
    }

    [Fact]
    public void Handle_BothOverdueAndDueToday_MentionsBoth()
    {
        var result = _handler.Handle(new DueSummaryResult(Overdue: 1, DueToday: 2));

        Assert.Equal("You have 1 overdue and 2 due today.", result);
    }

    [Fact]
    public void Handle_SingleOverdueItem_DoesNotPluralizeIncorrectly()
    {
        // The word "overdue" has no plural form, so wording stays identical for 1 vs many —
        // this test just locks in the exact count phrasing.
        var result = _handler.Handle(new DueSummaryResult(Overdue: 1, DueToday: 0));

        Assert.Equal("You have 1 overdue.", result);
    }
}
