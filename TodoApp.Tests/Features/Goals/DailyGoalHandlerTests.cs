using TodoApp.Features.Goals;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Goals;

public class DailyGoalHandlerTests
{
    [Fact]
    public async Task GetDailyGoal_ReturnsNull_WhenNoGoalSet()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new GetDailyGoalHandler(db);

        var result = await handler.HandleAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task SetDailyGoal_ThenGet_ReturnsTarget()
    {
        var db = await TestDatabase.CreateAsync();
        var set = new SetDailyGoalHandler(db);
        var get = new GetDailyGoalHandler(db);

        await set.HandleAsync(5);
        var result = await get.HandleAsync();

        Assert.Equal(5, result);
    }

    [Fact]
    public async Task SetDailyGoal_Overwrites_PreviousGoal()
    {
        var db = await TestDatabase.CreateAsync();
        var set = new SetDailyGoalHandler(db);
        var get = new GetDailyGoalHandler(db);

        await set.HandleAsync(3);
        await set.HandleAsync(10);
        var result = await get.HandleAsync();

        Assert.Equal(10, result);
    }

    [Fact]
    public async Task SetDailyGoal_ZeroTarget_ClearsGoal()
    {
        var db = await TestDatabase.CreateAsync();
        var set = new SetDailyGoalHandler(db);
        var get = new GetDailyGoalHandler(db);

        await set.HandleAsync(5);
        await set.HandleAsync(0);
        var result = await get.HandleAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task SetDailyGoal_NegativeTarget_Throws()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new SetDailyGoalHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(-1));
    }

    [Fact]
    public async Task SetDailyGoal_LargeTarget_IsStored()
    {
        var db = await TestDatabase.CreateAsync();
        var set = new SetDailyGoalHandler(db);
        var get = new GetDailyGoalHandler(db);

        await set.HandleAsync(100);
        var result = await get.HandleAsync();

        Assert.Equal(100, result);
    }
}
