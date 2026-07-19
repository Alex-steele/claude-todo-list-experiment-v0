using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using TodoApp.Components.Pages;
using TodoApp.Features.Lists;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.CalendarView;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Components.Pages;

public class CalendarTests : BunitContext
{
    private static BunitContext CreateBunitContext(TodoApp.Infrastructure.Database db)
    {
        var ctx = new BunitContext();
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddScoped(_ => db);
        ctx.Services.AddScoped<GetTodosHandler>();
        ctx.Services.AddScoped<GetListsHandler>();
        ctx.Services.AddScoped<CalendarViewHandler>();
        return ctx;
    }

    private static IRenderedComponent<Calendar> RenderCalendar(BunitContext ctx, int? listId = null)
    {
        ctx.Render<MudPopoverProvider>();
        if (listId is int id)
        {
            var nav = ctx.Services.GetRequiredService<NavigationManager>();
            nav.NavigateTo(nav.GetUriWithQueryParameter("listId", id));
        }
        return ctx.Render<Calendar>();
    }

    [Fact]
    public async Task Calendar_RendersCurrentMonthHeaderAndDayGrid()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);

        var cut = RenderCalendar(ctx);

        var today = DateTime.Today;
        Assert.Contains(today.ToString("MMMM yyyy"), cut.Markup);
        Assert.Contains("Sun", cut.Markup);
        Assert.Contains("Sat", cut.Markup);

        var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
        cut.WaitForAssertion(() =>
            Assert.Equal(daysInMonth, cut.FindAll(".calendar-day-number").Count));
    }

    [Fact]
    public async Task Calendar_NoTodosDueThisMonth_ShowsEmptyState()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);

        var cut = RenderCalendar(ctx);

        cut.WaitForAssertion(() => Assert.Contains("No todos due this month", cut.Markup));
    }

    [Fact]
    public async Task Calendar_TodoDueThisMonth_AppearsOnItsDayCell()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var dueDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        await addHandler.HandleAsync("Renew passport", dueDate: dueDate);

        var ctx = CreateBunitContext(db);
        var cut = RenderCalendar(ctx);

        cut.WaitForAssertion(() => Assert.Contains("Renew passport", cut.Markup));
        cut.WaitForAssertion(() => Assert.DoesNotContain("No todos due this month", cut.Markup));
    }

    [Fact]
    public async Task Calendar_NextMonthButton_AdvancesHeaderAndReloadsTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var thisMonthDue = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        await addHandler.HandleAsync("Due this month", dueDate: thisMonthDue);

        var ctx = CreateBunitContext(db);
        var cut = RenderCalendar(ctx);

        cut.WaitForAssertion(() => Assert.Contains("Due this month", cut.Markup));

        var nextButton = cut.Find(".calendar-next-btn");
        nextButton.Click();

        var expectedHeader = DateTime.Today.AddMonths(1).ToString("MMMM yyyy");
        cut.WaitForAssertion(() => Assert.Contains(expectedHeader, cut.Markup));
        cut.WaitForAssertion(() => Assert.DoesNotContain("Due this month", cut.Markup));
    }

    [Fact]
    public async Task Calendar_TodayButton_ReturnsToCurrentMonthAfterNavigating()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderCalendar(ctx);

        cut.Find(".calendar-next-btn").Click();
        cut.Find(".calendar-next-btn").Click();

        var todayButton = cut.Find(".calendar-today-btn");
        todayButton.Click();

        var currentHeader = DateTime.Today.ToString("MMMM yyyy");
        cut.WaitForAssertion(() => Assert.Contains(currentHeader, cut.Markup));
    }

    [Fact]
    public async Task Calendar_ScopesTodosToTheSelectedList()
    {
        var db = await TestDatabase.CreateAsync();
        var listHandler = new CreateListHandler(db);
        var otherListId = await listHandler.HandleAsync("Work");

        var addHandler = new AddTodoHandler(db);
        var dueThisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        await addHandler.HandleAsync("Personal list todo", dueDate: dueThisMonth, listId: 1);
        await addHandler.HandleAsync("Work list todo", dueDate: dueThisMonth, listId: otherListId);

        var ctx = CreateBunitContext(db);

        var cutForDefaultList = RenderCalendar(ctx);
        cutForDefaultList.WaitForAssertion(() => Assert.Contains("Personal list todo", cutForDefaultList.Markup));
        Assert.DoesNotContain("Work list todo", cutForDefaultList.Markup);

        var ctx2 = CreateBunitContext(db);
        var cutForOtherList = RenderCalendar(ctx2, otherListId);
        cutForOtherList.WaitForAssertion(() => Assert.Contains("Work list todo", cutForOtherList.Markup));
        Assert.DoesNotContain("Personal list todo", cutForOtherList.Markup);
    }
}
