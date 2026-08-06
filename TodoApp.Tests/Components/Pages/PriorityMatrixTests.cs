using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using TodoApp.Components.Pages;
using TodoApp.Features.Lists;
using TodoApp.Features.Todos;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.PriorityMatrix;
using TodoApp.Features.Todos.SetDueDate;
using TodoApp.Features.Todos.SetPriority;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Components.Pages;

public class PriorityMatrixTests : BunitContext
{
    private static BunitContext CreateBunitContext(TodoApp.Infrastructure.Database db)
    {
        var ctx = new BunitContext();
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddScoped(_ => db);
        ctx.Services.AddScoped<GetTodosHandler>();
        ctx.Services.AddScoped<GetListsHandler>();
        ctx.Services.AddScoped<PriorityMatrixHandler>();
        return ctx;
    }

    private static IRenderedComponent<PriorityMatrix> RenderPriorityMatrix(BunitContext ctx, int? listId = null)
    {
        ctx.Render<MudPopoverProvider>();
        if (listId is int id)
        {
            var nav = ctx.Services.GetRequiredService<NavigationManager>();
            nav.NavigateTo(nav.GetUriWithQueryParameter("listId", id));
        }
        return ctx.Render<PriorityMatrix>();
    }

    private static async Task<int> AddTodoWithPriorityAndDueDate(
        TodoApp.Infrastructure.Database db, string title, TodoPriority priority, DateTime? dueDate, int listId = 1)
    {
        var addHandler = new AddTodoHandler(db);
        var id = await addHandler.HandleAsync(title, listId: listId);

        if (priority != TodoPriority.None)
            await new SetPriorityHandler(db).HandleAsync(id, priority);
        if (dueDate is { } due)
            await new SetDueDateHandler(db).HandleAsync(id, due);

        return id;
    }

    [Fact]
    public async Task PriorityMatrix_NoActiveTodos_ShowsEmptyState()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);

        var cut = RenderPriorityMatrix(ctx);

        cut.WaitForAssertion(() =>
            Assert.Contains("No active todos to prioritize", cut.Markup));
        Assert.Empty(cut.FindAll(".priority-matrix-grid"));
    }

    [Fact]
    public async Task PriorityMatrix_HighPriorityOverdueTodo_AppearsInDoFirstQuadrant()
    {
        var db = await TestDatabase.CreateAsync();
        await AddTodoWithPriorityAndDueDate(db, "File taxes", TodoPriority.High, DateTime.Today.AddDays(-1));

        var ctx = CreateBunitContext(db);
        var cut = RenderPriorityMatrix(ctx);

        cut.WaitForAssertion(() =>
        {
            var quadrant = cut.Find(".priority-matrix-quadrant-do-first");
            Assert.Contains("File taxes", quadrant.TextContent);
        });
    }

    [Fact]
    public async Task PriorityMatrix_HighPriorityNoDueDate_AppearsInPlanQuadrant()
    {
        var db = await TestDatabase.CreateAsync();
        await AddTodoWithPriorityAndDueDate(db, "Learn Rust", TodoPriority.High, null);

        var ctx = CreateBunitContext(db);
        var cut = RenderPriorityMatrix(ctx);

        cut.WaitForAssertion(() =>
        {
            var quadrant = cut.Find(".priority-matrix-quadrant-plan");
            Assert.Contains("Learn Rust", quadrant.TextContent);
        });
    }

    [Fact]
    public async Task PriorityMatrix_LowPriorityDueToday_AppearsInDoQuicklyQuadrant()
    {
        var db = await TestDatabase.CreateAsync();
        await AddTodoWithPriorityAndDueDate(db, "Reply to email", TodoPriority.Low, DateTime.Today);

        var ctx = CreateBunitContext(db);
        var cut = RenderPriorityMatrix(ctx);

        cut.WaitForAssertion(() =>
        {
            var quadrant = cut.Find(".priority-matrix-quadrant-do-quickly");
            Assert.Contains("Reply to email", quadrant.TextContent);
        });
    }

    [Fact]
    public async Task PriorityMatrix_NoPriorityFutureDueDate_AppearsInSomedayQuadrant()
    {
        var db = await TestDatabase.CreateAsync();
        await AddTodoWithPriorityAndDueDate(db, "Reorganize garage", TodoPriority.None, DateTime.Today.AddDays(30));

        var ctx = CreateBunitContext(db);
        var cut = RenderPriorityMatrix(ctx);

        cut.WaitForAssertion(() =>
        {
            var quadrant = cut.Find(".priority-matrix-quadrant-someday");
            Assert.Contains("Reorganize garage", quadrant.TextContent);
        });
    }

    [Fact]
    public async Task PriorityMatrix_ScopesTodosToTheSelectedList()
    {
        var db = await TestDatabase.CreateAsync();
        var otherListId = await new CreateListHandler(db).HandleAsync("Work");

        await AddTodoWithPriorityAndDueDate(db, "Personal urgent", TodoPriority.High, DateTime.Today, listId: 1);
        await AddTodoWithPriorityAndDueDate(db, "Work urgent", TodoPriority.High, DateTime.Today, listId: otherListId);

        var ctx = CreateBunitContext(db);
        var cutForDefaultList = RenderPriorityMatrix(ctx);
        cutForDefaultList.WaitForAssertion(() =>
            Assert.Contains("Personal urgent", cutForDefaultList.Markup));
        Assert.DoesNotContain("Work urgent", cutForDefaultList.Markup);

        var ctx2 = CreateBunitContext(db);
        var cutForOtherList = RenderPriorityMatrix(ctx2, otherListId);
        cutForOtherList.WaitForAssertion(() =>
            Assert.Contains("Work urgent", cutForOtherList.Markup));
        Assert.DoesNotContain("Personal urgent", cutForOtherList.Markup);
    }

    [Fact]
    public async Task PriorityMatrix_SwitchingListViaDropdown_ReloadsMatrixForNewList()
    {
        var db = await TestDatabase.CreateAsync();
        var otherListId = await new CreateListHandler(db).HandleAsync("Work");
        await AddTodoWithPriorityAndDueDate(db, "Personal urgent", TodoPriority.High, DateTime.Today, listId: 1);

        var ctx = CreateBunitContext(db);
        var cut = RenderPriorityMatrix(ctx);

        cut.WaitForAssertion(() => Assert.Contains("Personal urgent", cut.Markup));

        var select = cut.FindComponent<MudSelect<int>>();
        await cut.InvokeAsync(() => select.Instance.ValueChanged.InvokeAsync(otherListId));

        cut.WaitForAssertion(() =>
            Assert.Contains("No active todos to prioritize", cut.Markup));
    }

    [Fact]
    public async Task PriorityMatrix_CompletedTodo_DoesNotAppearInAnyQuadrant()
    {
        var db = await TestDatabase.CreateAsync();
        var id = await AddTodoWithPriorityAndDueDate(db, "Done already", TodoPriority.High, DateTime.Today.AddDays(-1));
        await new TodoApp.Features.Todos.CompleteTodo.CompleteTodoHandler(db).HandleAsync(id);

        var ctx = CreateBunitContext(db);
        var cut = RenderPriorityMatrix(ctx);

        cut.WaitForAssertion(() =>
            Assert.Contains("No active todos to prioritize", cut.Markup));
    }
}
