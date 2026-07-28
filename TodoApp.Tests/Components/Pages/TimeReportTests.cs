using Bunit;
using Dapper;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using TodoApp.Components.Pages;
using TodoApp.Features.Lists;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.TimeReport;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Components.Pages;

public class TimeReportTests : BunitContext
{
    private static BunitContext CreateBunitContext(TodoApp.Infrastructure.Database db)
    {
        var ctx = new BunitContext();
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddScoped(_ => db);
        ctx.Services.AddScoped<GetTodosHandler>();
        ctx.Services.AddScoped<GetListsHandler>();
        ctx.Services.AddScoped<TimeReportHandler>();
        return ctx;
    }

    private static IRenderedComponent<TimeReport> RenderTimeReport(BunitContext ctx, int? listId = null)
    {
        ctx.Render<MudPopoverProvider>();
        if (listId is int id)
        {
            var nav = ctx.Services.GetRequiredService<NavigationManager>();
            nav.NavigateTo(nav.GetUriWithQueryParameter("listId", id));
        }
        return ctx.Render<TimeReport>();
    }

    private static async Task<int> AddWithTrackedTime(
        TodoApp.Infrastructure.Database db, string title, int timeSpentSeconds, int listId = 1)
    {
        var addHandler = new AddTodoHandler(db);
        var id = await addHandler.HandleAsync(title, listId: listId);
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Todos SET TimeSpentSeconds = @seconds WHERE Id = @id",
            new { seconds = timeSpentSeconds, id });
        return id;
    }

    [Fact]
    public async Task TimeReport_NoTrackedTime_ShowsEmptyState()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);

        var cut = RenderTimeReport(ctx);

        cut.WaitForAssertion(() =>
            Assert.Contains("Track time on a todo to see a breakdown here.", cut.Markup));
        Assert.Empty(cut.FindAll(".time-report-list"));
    }

    [Fact]
    public async Task TimeReport_WithTrackedTime_ShowsTotalAndRows()
    {
        var db = await TestDatabase.CreateAsync();
        await AddWithTrackedTime(db, "Write report", 125); // 2m

        var ctx = CreateBunitContext(db);
        var cut = RenderTimeReport(ctx);

        cut.WaitForAssertion(() => Assert.Contains("Write report", cut.Markup));
        Assert.Contains("Total time tracked:", cut.Markup);
        Assert.Contains("2m", cut.Markup);
        Assert.Single(cut.FindAll(".time-report-row"));
    }

    [Fact]
    public async Task TimeReport_MultipleTodos_SortsRowsByTimeDescending()
    {
        var db = await TestDatabase.CreateAsync();
        await AddWithTrackedTime(db, "Short task", 30);
        await AddWithTrackedTime(db, "Long task", 600);

        var ctx = CreateBunitContext(db);
        var cut = RenderTimeReport(ctx);

        cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll(".time-report-row").Count));
        var titles = cut.FindAll(".time-report-row-title").Select(e => e.TextContent.Trim()).ToList();
        Assert.Equal("Long task", titles[0]);
        Assert.Equal("Short task", titles[1]);
    }

    [Fact]
    public async Task TimeReport_ScopesToSelectedList()
    {
        var db = await TestDatabase.CreateAsync();
        var listHandler = new CreateListHandler(db);
        var otherListId = await listHandler.HandleAsync("Work");

        await AddWithTrackedTime(db, "Personal task", 60, listId: 1);
        await AddWithTrackedTime(db, "Work task", 90, listId: otherListId);

        var ctx = CreateBunitContext(db);
        var cutForDefaultList = RenderTimeReport(ctx);
        cutForDefaultList.WaitForAssertion(() => Assert.Contains("Personal task", cutForDefaultList.Markup));
        Assert.DoesNotContain("Work task", cutForDefaultList.Markup);

        var ctx2 = CreateBunitContext(db);
        var cutForOtherList = RenderTimeReport(ctx2, otherListId);
        cutForOtherList.WaitForAssertion(() => Assert.Contains("Work task", cutForOtherList.Markup));
        Assert.DoesNotContain("Personal task", cutForOtherList.Markup);
    }

    [Fact]
    public async Task TimeReport_SwitchingListViaDropdown_ReloadsReportForNewList()
    {
        var db = await TestDatabase.CreateAsync();
        var listHandler = new CreateListHandler(db);
        var otherListId = await listHandler.HandleAsync("Work");

        await AddWithTrackedTime(db, "Personal task", 60, listId: 1);

        var ctx = CreateBunitContext(db);
        var cut = RenderTimeReport(ctx);

        cut.WaitForAssertion(() => Assert.Contains("Personal task", cut.Markup));

        var select = cut.FindComponent<MudSelect<int>>();
        await cut.InvokeAsync(() => select.Instance.ValueChanged.InvokeAsync(otherListId));

        cut.WaitForAssertion(() =>
            Assert.Contains("Track time on a todo to see a breakdown here.", cut.Markup));
    }
}
