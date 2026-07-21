using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using TodoApp.Components.Pages;
using TodoApp.Features.Lists;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.CalendarView;
using TodoApp.Features.Todos.CompleteTodo;
using TodoApp.Features.Todos.DeleteTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.SetDueDate;
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
        ctx.Services.AddScoped<AddTodoHandler>();
        ctx.Services.AddScoped<SetDueDateHandler>();
        ctx.Services.AddScoped<CompleteTodoHandler>();
        ctx.Services.AddScoped<DeleteTodoHandler>();
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

    // Quick-add-from-calendar tests

    [Fact]
    public async Task Calendar_ClickingAddButtonOnDay_ShowsInlineInput()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderCalendar(ctx);

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".calendar-day-add-btn")));
        cut.FindAll(".calendar-day-add-btn")[0].Click();

        Assert.NotEmpty(cut.FindAll(".calendar-day-add-input"));
    }

    [Fact]
    public async Task Calendar_AddTodoViaInlineForm_CreatesTodoDueOnThatDay()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderCalendar(ctx);

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".calendar-day-add-btn")));
        cut.FindAll(".calendar-day-add-btn")[0].Click();

        cut.Find(".calendar-day-add-input input").Input("Renew passport");
        cut.Find(".calendar-day-add-input input").KeyUp(Key.Enter);

        cut.WaitForAssertion(() => Assert.Contains("Renew passport", cut.Markup), TimeSpan.FromSeconds(5));
        Assert.Empty(cut.FindAll(".calendar-day-add-input"));
    }

    [Fact]
    public async Task Calendar_EscapeKey_CancelsInlineAddWithoutCreatingTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderCalendar(ctx);

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".calendar-day-add-btn")));
        cut.FindAll(".calendar-day-add-btn")[0].Click();

        cut.Find(".calendar-day-add-input input").Input("Should not be saved");
        cut.Find(".calendar-day-add-input input").KeyUp(Key.Escape);

        Assert.Empty(cut.FindAll(".calendar-day-add-input"));
        Assert.DoesNotContain("Should not be saved", cut.Markup);
    }

    [Fact]
    public async Task Calendar_EnterWithEmptyTitle_DoesNotCreateTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var getTodosHandler = new GetTodosHandler(db);
        var ctx = CreateBunitContext(db);
        var cut = RenderCalendar(ctx);

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".calendar-day-add-btn")));
        cut.FindAll(".calendar-day-add-btn")[0].Click();

        cut.Find(".calendar-day-add-input input").KeyUp(Key.Enter);

        Assert.Empty(cut.FindAll(".calendar-day-add-input"));
        var todos = await getTodosHandler.HandleAsync();
        Assert.Empty(todos);
    }

    // Drag-to-reschedule tests

    [Fact]
    public async Task Calendar_TodoOnDayCell_IsDraggable()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var dueDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        await addHandler.HandleAsync("Renew passport", dueDate: dueDate);

        var ctx = CreateBunitContext(db);
        var cut = RenderCalendar(ctx);

        cut.WaitForAssertion(() => Assert.Contains("Renew passport", cut.Markup));

        var todoElement = cut.Find(".calendar-day-todo");
        Assert.Equal("true", todoElement.GetAttribute("draggable"));
    }

    [Fact]
    public async Task Calendar_DragTodoToAnotherDay_UpdatesDueDate()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getTodosHandler = new GetTodosHandler(db);
        var firstOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        await addHandler.HandleAsync("Renew passport", dueDate: firstOfMonth);

        var ctx = CreateBunitContext(db);
        var cut = RenderCalendar(ctx);

        cut.WaitForAssertion(() => Assert.Contains("Renew passport", cut.Markup));

        cut.Find(".calendar-day-todo").DragStart();

        // Re-query after DragStart triggers a render, per bUnit's stale-element guidance.
        // dayCells[0] is day 1 (where the todo lives), dayCells[1] is day 2 (drop target).
        var dayCells = cut.FindAll(".calendar-day-number");
        dayCells[1].Closest(".calendar-day")!.Drop();

        await cut.WaitForAssertionAsync(async () =>
        {
            var todos = await getTodosHandler.HandleAsync();
            var todo = Assert.Single(todos);
            Assert.Equal(firstOfMonth.AddDays(1), todo.DueDate!.Value.Date);
        }, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Calendar_DropOnSameDay_LeavesDueDateUnchanged()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getTodosHandler = new GetTodosHandler(db);
        var firstOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        await addHandler.HandleAsync("Renew passport", dueDate: firstOfMonth);

        var ctx = CreateBunitContext(db);
        var cut = RenderCalendar(ctx);

        cut.WaitForAssertion(() => Assert.Contains("Renew passport", cut.Markup));

        var todoElement = cut.Find(".calendar-day-todo");
        todoElement.DragStart();
        todoElement.Closest(".calendar-day")!.Drop();

        var todos = await getTodosHandler.HandleAsync();
        var todo = Assert.Single(todos);
        Assert.Equal(firstOfMonth, todo.DueDate!.Value.Date);
    }

    // Day-detail panel tests

    [Fact]
    public async Task Calendar_ClickingDayNumber_OpensDetailPanelListingAllTodosForThatDay()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var firstOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        await addHandler.HandleAsync("Renew passport", dueDate: firstOfMonth);
        await addHandler.HandleAsync("Pay rent", dueDate: firstOfMonth);
        await addHandler.HandleAsync("Call dentist", dueDate: firstOfMonth);
        await addHandler.HandleAsync("Water plants", dueDate: firstOfMonth);

        var ctx = CreateBunitContext(db);
        var cut = RenderCalendar(ctx);

        cut.WaitForAssertion(() => Assert.Contains("+1 more", cut.Markup));

        cut.Find(".calendar-day-number-btn").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains(firstOfMonth.ToString("dddd, MMMM d"), cut.Markup);
            Assert.Equal(4, cut.FindAll(".calendar-day-detail-row").Count);
            Assert.Contains("Water plants", cut.Markup);
        });
    }

    [Fact]
    public async Task Calendar_ClickingSameDayNumberTwice_ClosesDetailPanel()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var firstOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        await addHandler.HandleAsync("Renew passport", dueDate: firstOfMonth);

        var ctx = CreateBunitContext(db);
        var cut = RenderCalendar(ctx);

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".calendar-day-number-btn")));
        cut.Find(".calendar-day-number-btn").Click();
        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".calendar-day-detail")));

        cut.Find(".calendar-day-number-btn").Click();
        Assert.Empty(cut.FindAll(".calendar-day-detail"));
    }

    [Fact]
    public async Task Calendar_CloseButtonOnDetailPanel_HidesPanel()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderCalendar(ctx);

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".calendar-day-number-btn")));
        cut.Find(".calendar-day-number-btn").Click();
        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".calendar-day-detail")));

        cut.Find(".calendar-day-detail-close").Click();
        Assert.Empty(cut.FindAll(".calendar-day-detail"));
    }

    [Fact]
    public async Task Calendar_ClickingMoreLink_OpensDetailPanelForThatDay()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var firstOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        await addHandler.HandleAsync("Todo 1", dueDate: firstOfMonth);
        await addHandler.HandleAsync("Todo 2", dueDate: firstOfMonth);
        await addHandler.HandleAsync("Todo 3", dueDate: firstOfMonth);
        await addHandler.HandleAsync("Todo 4", dueDate: firstOfMonth);

        var ctx = CreateBunitContext(db);
        var cut = RenderCalendar(ctx);

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".calendar-day-more")));
        cut.Find(".calendar-day-more").Click();

        cut.WaitForAssertion(() => Assert.Equal(4, cut.FindAll(".calendar-day-detail-row").Count));
    }

    [Fact]
    public async Task Calendar_DetailPanelForEmptyDay_ShowsEmptyMessage()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderCalendar(ctx);

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".calendar-day-number-btn")));
        cut.Find(".calendar-day-number-btn").Click();

        cut.WaitForAssertion(() => Assert.Contains("No todos due this day", cut.Markup));
    }

    [Fact]
    public async Task Calendar_TogglingCheckboxInDetailPanel_CompletesTodoAndUpdatesGrid()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getTodosHandler = new GetTodosHandler(db);
        var firstOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        await addHandler.HandleAsync("Renew passport", dueDate: firstOfMonth);

        var ctx = CreateBunitContext(db);
        var cut = RenderCalendar(ctx);

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".calendar-day-number-btn")));
        cut.Find(".calendar-day-number-btn").Click();
        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".calendar-day-detail-checkbox input")));

        cut.Find(".calendar-day-detail-checkbox input").Change(true);

        await cut.WaitForAssertionAsync(async () =>
        {
            var todos = await getTodosHandler.HandleAsync();
            Assert.True(Assert.Single(todos).IsCompleted);
        });
        cut.WaitForAssertion(() => Assert.Contains("calendar-day-todo-completed", cut.Markup));
    }

    [Fact]
    public async Task Calendar_DeletingTodoFromDetailPanel_RemovesItFromPanelAndGrid()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getTodosHandler = new GetTodosHandler(db);
        var firstOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        await addHandler.HandleAsync("Renew passport", dueDate: firstOfMonth);

        var ctx = CreateBunitContext(db);
        var cut = RenderCalendar(ctx);

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".calendar-day-number-btn")));
        cut.Find(".calendar-day-number-btn").Click();
        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".calendar-day-detail-delete")));

        cut.Find(".calendar-day-detail-delete").Click();

        await cut.WaitForAssertionAsync(async () =>
        {
            var todos = await getTodosHandler.HandleAsync();
            Assert.Empty(todos);
        });
        cut.WaitForAssertion(() => Assert.DoesNotContain("Renew passport", cut.Markup));
    }
}
