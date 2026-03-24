using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using TodoApp.Components.Pages;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.CompleteTodo;
using TodoApp.Features.Todos.DeleteTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Components.Pages;

public class HomeTests : BunitContext
{
    private static BunitContext CreateBunitContext(TodoApp.Infrastructure.Database db)
    {
        var ctx = new BunitContext();
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddScoped(_ => db);
        ctx.Services.AddScoped<AddTodoHandler>();
        ctx.Services.AddScoped<CompleteTodoHandler>();
        ctx.Services.AddScoped<DeleteTodoHandler>();
        ctx.Services.AddScoped<GetTodosHandler>();
        return ctx;
    }

    private static IRenderedComponent<Home> RenderHome(BunitContext ctx)
    {
        // MudBlazor requires MudPopoverProvider in the render tree
        ctx.Render<MudPopoverProvider>();
        return ctx.Render<Home>();
    }

    [Fact]
    public async Task DueDatePicker_IsRendered()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);

        var datePicker = cut.FindComponent<MudDatePicker>();
        Assert.NotNull(datePicker);
    }

    [Fact]
    public async Task AddTodo_WithDueDate_ShowsDueDateInList()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var dueDate = DateTime.Today.AddDays(5);
        await addHandler.HandleAsync("Test todo with due date", dueDate: dueDate);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        var markup = cut.Markup;
        Assert.Contains(dueDate.ToString("MMM d, yyyy"), markup);
    }

    [Fact]
    public async Task AddTodo_WithoutDueDate_ShowsNoDueDate()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Todo without due date");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        var markup = cut.Markup;
        Assert.Contains("Todo without due date", markup);
        // The due date display uses "Due MMM d, yyyy" format - no date should appear in the list items
        // We verify by checking the todo list area does not contain a due date line
        var todoItems = cut.FindAll("li");
        Assert.All(todoItems, item => Assert.DoesNotContain("Due ", item.InnerHtml.Split("Due date")[0]));
    }

    [Fact]
    public async Task OverdueTodo_ShowsWarningIndicator()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var pastDueDate = DateTime.Today.AddDays(-3);
        await addHandler.HandleAsync("Overdue task", dueDate: pastDueDate);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        var markup = cut.Markup;
        Assert.Contains(pastDueDate.ToString("MMM d, yyyy"), markup);
        // MudBlazor renders Color.Error text with mud-error-text CSS class
        Assert.Contains("mud-error-text", markup);
    }
}
