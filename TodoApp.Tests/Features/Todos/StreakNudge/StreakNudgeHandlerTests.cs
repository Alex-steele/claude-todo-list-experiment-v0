using TodoApp.Features.Todos.StreakNudge;
using Xunit;

namespace TodoApp.Tests.Features.Todos.StreakNudge;

public class StreakNudgeHandlerTests
{
    private readonly StreakNudgeHandler _handler = new();

    [Fact]
    public void ShouldNudge_ReturnsFalse_WhenNoStreak()
    {
        Assert.False(_handler.ShouldNudge(currentStreak: 0, completedToday: 0));
    }

    [Fact]
    public void ShouldNudge_ReturnsTrue_WhenStreakActiveAndNothingDoneToday()
    {
        Assert.True(_handler.ShouldNudge(currentStreak: 3, completedToday: 0));
    }

    [Fact]
    public void ShouldNudge_ReturnsFalse_WhenStreakActiveAndTodoCompletedToday()
    {
        Assert.False(_handler.ShouldNudge(currentStreak: 5, completedToday: 1));
    }

    [Fact]
    public void ShouldNudge_ReturnsFalse_WhenMultipleCompletedToday()
    {
        Assert.False(_handler.ShouldNudge(currentStreak: 10, completedToday: 4));
    }

    [Fact]
    public void ShouldNudge_ReturnsTrue_ForStreakOfOne()
    {
        Assert.True(_handler.ShouldNudge(currentStreak: 1, completedToday: 0));
    }
}
