using Bunit;
using Dapper;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using TodoApp.Components.Pages;
using TodoApp.Features.Lists;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.CompleteTodo;
using TodoApp.Features.Todos.DayOfWeekStats;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Components.Pages;

public class AnalyticsTests : BunitContext
{
    private static BunitContext CreateBunitContext(TodoApp.Infrastructure.Database db)
    {
        var ctx = new BunitContext();
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddScoped(_ => db);
        ctx.Services.AddScoped<GetTodosHandler>();
        ctx.Services.AddScoped<GetListsHandler>();
        ctx.Services.AddScoped<DayOfWeekStatsHandler>();
        return ctx;
    }

    private static IRenderedComponent<Analytics> RenderAnalytics(BunitContext ctx, int? listId = null)
    {
        ctx.Render<MudPopoverProvider>();
        if (listId is int id)
        {
            var nav = ctx.Services.GetRequiredService<NavigationManager>();
            nav.NavigateTo(nav.GetUriWithQueryParameter("listId", id));
        }
        return ctx.Render<Analytics>();
    }

    private static async Task<int> AddAndCompleteOn(
        TodoApp.Infrastructure.Database db, string title, DateTime completedAt, int listId = 1)
    {
        var addHandler = new AddTodoHandler(db);
        var id = await addHandler.HandleAsync(title, listId: listId);
        // CompleteTodoHandler always stamps "now"; backdate directly so we can control the day of week.
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Todos SET IsCompleted = 1, CompletedAt = @completedAt WHERE Id = @id",
            new { completedAt = completedAt.ToString("O"), id });
        return id;
    }

    [Fact]
    public async Task Analytics_NoCompletions_ShowsEmptyState()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);

        var cut = RenderAnalytics(ctx);

        cut.WaitForAssertion(() =>
            Assert.Contains("Complete some todos to see your productivity patterns here.", cut.Markup));
        Assert.Empty(cut.FindAll(".dow-chart"));
    }

    [Fact]
    public async Task Analytics_WithCompletions_RendersSevenDayColumns()
    {
        var db = await TestDatabase.CreateAsync();
        var tuesday = new DateTime(2026, 7, 21);
        await AddAndCompleteOn(db, "Renew passport", tuesday);

        var ctx = CreateBunitContext(db);
        var cut = RenderAnalytics(ctx);

        cut.WaitForAssertion(() => Assert.Equal(7, cut.FindAll(".dow-bar-column").Count));
        Assert.Contains("Sun", cut.Markup);
        Assert.Contains("Tue", cut.Markup);
    }

    [Fact]
    public async Task Analytics_SingleCompletion_HighlightsCorrectDayAsBest()
    {
        var db = await TestDatabase.CreateAsync();
        var tuesday = new DateTime(2026, 7, 21); // Tuesday
        await AddAndCompleteOn(db, "Renew passport", tuesday);

        var ctx = CreateBunitContext(db);
        var cut = RenderAnalytics(ctx);

        cut.WaitForAssertion(() => Assert.Contains("dow-bar-best", cut.Markup));
        cut.WaitForAssertion(() =>
            Assert.Contains("Your most productive day is", cut.Markup));
        cut.WaitForAssertion(() => Assert.Contains("Tuesday", cut.Markup));
    }

    [Fact]
    public async Task Analytics_ScopesCompletionsToTheSelectedList()
    {
        var db = await TestDatabase.CreateAsync();
        var listHandler = new CreateListHandler(db);
        var otherListId = await listHandler.HandleAsync("Work");

        var tuesday = new DateTime(2026, 7, 21);
        await AddAndCompleteOn(db, "Personal list todo", tuesday, listId: 1);

        var wednesday = new DateTime(2026, 7, 22);
        await AddAndCompleteOn(db, "Work list todo", wednesday, listId: otherListId);

        var ctx = CreateBunitContext(db);
        var cutForDefaultList = RenderAnalytics(ctx);
        cutForDefaultList.WaitForAssertion(() =>
            Assert.Contains("Your most productive day is <strong>Tuesday</strong>", cutForDefaultList.Markup));
        Assert.DoesNotContain("Your most productive day is <strong>Wednesday</strong>", cutForDefaultList.Markup);

        var ctx2 = CreateBunitContext(db);
        var cutForOtherList = RenderAnalytics(ctx2, otherListId);
        cutForOtherList.WaitForAssertion(() =>
            Assert.Contains("Your most productive day is <strong>Wednesday</strong>", cutForOtherList.Markup));
        Assert.DoesNotContain("Your most productive day is <strong>Tuesday</strong>", cutForOtherList.Markup);
    }

    [Fact]
    public async Task Analytics_MultipleCompletionsOnSameDay_CountAccumulatesOnThatBar()
    {
        var db = await TestDatabase.CreateAsync();
        var wednesday = new DateTime(2026, 7, 22);
        await AddAndCompleteOn(db, "Todo A", wednesday);
        await AddAndCompleteOn(db, "Todo B", wednesday);

        var ctx = CreateBunitContext(db);
        var cut = RenderAnalytics(ctx);

        cut.WaitForAssertion(() =>
        {
            var counts = cut.FindAll(".dow-bar-count");
            Assert.Contains(counts, c => c.TextContent.Trim() == "2");
        });
    }

    [Fact]
    public async Task Analytics_SwitchingListViaDropdown_ReloadsStatsForNewList()
    {
        var db = await TestDatabase.CreateAsync();
        var listHandler = new CreateListHandler(db);
        var otherListId = await listHandler.HandleAsync("Work");

        var tuesday = new DateTime(2026, 7, 21);
        await AddAndCompleteOn(db, "Personal list todo", tuesday, listId: 1);

        var ctx = CreateBunitContext(db);
        var cut = RenderAnalytics(ctx);

        cut.WaitForAssertion(() => Assert.Contains("Tuesday", cut.Markup));

        var select = cut.FindComponent<MudSelect<int>>();
        await cut.InvokeAsync(() => select.Instance.ValueChanged.InvokeAsync(otherListId));

        cut.WaitForAssertion(() =>
            Assert.Contains("Complete some todos to see your productivity patterns here.", cut.Markup));
    }
}
