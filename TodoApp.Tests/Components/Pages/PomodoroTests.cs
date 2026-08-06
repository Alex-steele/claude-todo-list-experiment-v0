using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using TodoApp.Components.Pages;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.PomodoroTimer;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Components.Pages;

public class PomodoroTests : BunitContext
{
    private static BunitContext CreateBunitContext(TodoApp.Infrastructure.Database db)
    {
        var ctx = new BunitContext();
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddScoped(_ => db);
        ctx.Services.AddScoped<LogPomodoroSessionHandler>();
        ctx.Services.AddScoped<GetTodaysPomodoroCountHandler>();
        ctx.Services.AddScoped<GetTodosHandler>();
        ctx.Services.AddScoped<GetPomodoroSessionCountsHandler>();
        return ctx;
    }

    private static IRenderedComponent<Pomodoro> RenderPomodoro(BunitContext ctx)
    {
        ctx.Render<MudPopoverProvider>();
        return ctx.Render<Pomodoro>();
    }

    [Fact]
    public async Task Pomodoro_InitialRender_ShowsWorkPhaseFullCountdownAndStartButton()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);

        var cut = RenderPomodoro(ctx);

        Assert.Contains("Work", cut.Find(".pomodoro-phase-chip").TextContent);
        Assert.Equal("25:00", cut.Find(".pomodoro-countdown").TextContent);
        Assert.NotEmpty(cut.FindAll(".pomodoro-start-btn"));
        Assert.Empty(cut.FindAll(".pomodoro-pause-btn"));
        Assert.Contains("0", cut.Find(".pomodoro-session-count").TextContent);
    }

    [Fact]
    public async Task Pomodoro_SessionsLoggedEarlierToday_AreReflectedInInitialCount()
    {
        var db = await TestDatabase.CreateAsync();
        var logHandler = new LogPomodoroSessionHandler(db);
        await logHandler.HandleAsync();
        await logHandler.HandleAsync();

        var ctx = CreateBunitContext(db);
        var cut = RenderPomodoro(ctx);

        cut.WaitForAssertion(() =>
            Assert.Contains("2", cut.Find(".pomodoro-session-count").TextContent));
    }

    [Fact]
    public async Task Pomodoro_ClickStart_SwitchesControlsToPause()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderPomodoro(ctx);

        cut.Find(".pomodoro-start-btn").Click();

        Assert.NotEmpty(cut.FindAll(".pomodoro-pause-btn"));
        Assert.Empty(cut.FindAll(".pomodoro-start-btn"));
    }

    [Fact]
    public async Task Pomodoro_ClickSkip_AdvancesToBreakAndLogsCompletedWorkSession()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderPomodoro(ctx);

        cut.Find(".pomodoro-skip-btn").Click();

        cut.WaitForAssertion(() =>
            Assert.Contains("Break", cut.Find(".pomodoro-phase-chip").TextContent));
        Assert.Equal("5:00", cut.Find(".pomodoro-countdown").TextContent);
        Assert.Contains("1", cut.Find(".pomodoro-session-count").TextContent);

        using var conn = db.CreateConnection();
        var count = await Dapper.SqlMapper.ExecuteScalarAsync<int>(conn, "SELECT COUNT(*) FROM PomodoroSessions");
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Pomodoro_SkipDuringBreak_DoesNotLogAnAdditionalSession()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderPomodoro(ctx);

        cut.Find(".pomodoro-skip-btn").Click(); // Work -> Break (logs 1)
        cut.WaitForAssertion(() =>
            Assert.Contains("Break", cut.Find(".pomodoro-phase-chip").TextContent));

        cut.Find(".pomodoro-skip-btn").Click(); // Break -> Work (no log)
        cut.WaitForAssertion(() =>
            Assert.Contains("Work", cut.Find(".pomodoro-phase-chip").TextContent));

        Assert.Contains("1", cut.Find(".pomodoro-session-count").TextContent);
    }

    [Fact]
    public async Task Pomodoro_ClickReset_ReturnsToFullDurationAndStartButton()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderPomodoro(ctx);

        cut.Find(".pomodoro-start-btn").Click();
        cut.Find(".pomodoro-pause-btn").Click();
        cut.Find(".pomodoro-reset-btn").Click();

        Assert.Equal("25:00", cut.Find(".pomodoro-countdown").TextContent);
        Assert.NotEmpty(cut.FindAll(".pomodoro-start-btn"));
    }

    [Fact]
    public async Task Pomodoro_NoTodosInList_DoesNotRenderTodoSelect()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);

        var cut = RenderPomodoro(ctx);

        Assert.Empty(cut.FindAll(".pomodoro-todo-select"));
    }

    [Fact]
    public async Task Pomodoro_WithTodosInList_RendersTodoSelectDefaultingToNoSpecificTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Write report");

        var ctx = CreateBunitContext(db);
        var cut = RenderPomodoro(ctx);

        Assert.NotEmpty(cut.FindAll(".pomodoro-todo-select"));
        Assert.Empty(cut.FindAll(".pomodoro-todo-session-count"));
    }

    [Fact]
    public async Task Pomodoro_SelectingTodo_ShowsItsFocusSessionCount()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var todoId = await addHandler.HandleAsync("Write report");
        var logHandler = new LogPomodoroSessionHandler(db);
        await logHandler.HandleAsync(todoId);
        await logHandler.HandleAsync(todoId);

        var ctx = CreateBunitContext(db);
        var cut = RenderPomodoro(ctx);

        var select = cut.FindComponent<MudSelect<int>>();
        await cut.InvokeAsync(() => select.Instance.ValueChanged.InvokeAsync(todoId));

        cut.WaitForAssertion(() =>
            Assert.Contains("2", cut.Find(".pomodoro-todo-session-count").TextContent));
    }

    [Fact]
    public async Task Pomodoro_SkipWithTodoSelected_LogsSessionAgainstThatTodoAndUpdatesCount()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var todoId = await addHandler.HandleAsync("Write report");

        var ctx = CreateBunitContext(db);
        var cut = RenderPomodoro(ctx);

        var select = cut.FindComponent<MudSelect<int>>();
        await cut.InvokeAsync(() => select.Instance.ValueChanged.InvokeAsync(todoId));
        cut.WaitForAssertion(() =>
            Assert.Contains("0", cut.Find(".pomodoro-todo-session-count").TextContent));

        cut.Find(".pomodoro-skip-btn").Click();

        cut.WaitForAssertion(() =>
            Assert.Contains("1", cut.Find(".pomodoro-todo-session-count").TextContent));

        var counts = await new GetPomodoroSessionCountsHandler(db).HandleAsync([todoId]);
        Assert.Equal(1, counts[todoId]);
    }

    [Fact]
    public async Task Pomodoro_SkipWithNoTodoSelected_LogsSessionWithoutATodoId()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var todoId = await addHandler.HandleAsync("Write report");

        var ctx = CreateBunitContext(db);
        var cut = RenderPomodoro(ctx);

        cut.Find(".pomodoro-skip-btn").Click();

        cut.WaitForAssertion(() =>
            Assert.Contains("Break", cut.Find(".pomodoro-phase-chip").TextContent));

        var counts = await new GetPomodoroSessionCountsHandler(db).HandleAsync([todoId]);
        Assert.False(counts.ContainsKey(todoId));
    }
}
