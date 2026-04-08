using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using TodoApp.Components.Pages;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.BulkOperations;
using TodoApp.Features.Todos.CompleteTodo;
using TodoApp.Features.Todos.DeleteTodo;
using TodoApp.Features.Todos.EditTodo;
using TodoApp.Features.Todos.FilterSortTodos;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.GetTodosStats;
using TodoApp.Features.Todos.UndoRedo;
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
        ctx.Services.AddScoped<EditTodoHandler>();
        ctx.Services.AddScoped<GetTodosHandler>();
        ctx.Services.AddScoped<FilterSortTodosHandler>();
        ctx.Services.AddScoped<BulkOperationsHandler>();
        ctx.Services.AddScoped<RestoreTodosHandler>();
        ctx.Services.AddScoped<GetTodosStatsHandler>();
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

    [Fact]
    public async Task FilterControls_AreRendered_WhenTodosExist()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Some todo");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // Search field and filter buttons should be visible
        Assert.Contains("Search todos", cut.Markup);
        Assert.Contains("Active", cut.Markup);
        Assert.Contains("Completed", cut.Markup);
    }

    [Fact]
    public async Task FilterControls_AreNotRendered_WhenNoTodosExist()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.DoesNotContain("Search todos", cut.Markup);
        Assert.Contains("No todos yet", cut.Markup);
    }

    [Fact]
    public async Task FilterByActive_ShowsOnlyActiveTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var completeHandler = new CompleteTodoHandler(db);

        var activeId = await addHandler.HandleAsync("Active task");
        var completedId = await addHandler.HandleAsync("Completed task");
        await completeHandler.HandleAsync(completedId);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // Both todos should appear initially
        Assert.Contains("Active task", cut.Markup);
        Assert.Contains("Completed task", cut.Markup);

        // Click the "Active" filter button
        var activeButton = cut.FindAll("button").First(b => b.TextContent.Trim() == "Active");
        activeButton.Click();

        Assert.Contains("Active task", cut.Markup);
        Assert.DoesNotContain("Completed task", cut.Markup);
    }

    [Fact]
    public async Task FilterByCompleted_ShowsOnlyCompletedTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var completeHandler = new CompleteTodoHandler(db);

        await addHandler.HandleAsync("Active task");
        var completedId = await addHandler.HandleAsync("Completed task");
        await completeHandler.HandleAsync(completedId);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // Click the "Completed" filter button
        var completedButton = cut.FindAll("button").First(b => b.TextContent.Trim() == "Completed");
        completedButton.Click();

        Assert.DoesNotContain("Active task", cut.Markup);
        Assert.Contains("Completed task", cut.Markup);
    }

    [Fact]
    public async Task Search_FiltersTodosByTitle()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Buy groceries");
        await addHandler.HandleAsync("Walk the dog");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // Both should be visible initially
        Assert.Contains("Buy groceries", cut.Markup);
        Assert.Contains("Walk the dog", cut.Markup);

        // Type in the search box (identified by the todo-search-field CSS class)
        var searchInput = cut.Find(".todo-search-field input");
        searchInput.Input("groceries");

        Assert.Contains("Buy groceries", cut.Markup);
        Assert.DoesNotContain("Walk the dog", cut.Markup);
    }

    [Fact]
    public async Task FilterWithNoMatches_ShowsNoMatchMessage()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Buy groceries");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        var searchInput = cut.Find(".todo-search-field input");
        searchInput.Input("zzznomatch");

        Assert.Contains("No todos match your filters", cut.Markup);
    }

    [Fact]
    public async Task EditButton_IsRendered_ForEachTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Walk the dog");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("todo-edit-btn", cut.Markup);
    }

    [Fact]
    public async Task ClickingEditButton_ShowsEditField()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Walk the dog");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        var editBtn = cut.Find(".todo-edit-btn");
        editBtn.Click();

        Assert.Contains("todo-edit-field", cut.Markup);
    }

    [Fact]
    public async Task ClickingCancelEdit_HidesEditField()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Walk the dog");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // Start editing
        cut.Find(".todo-edit-btn").Click();
        Assert.Contains("todo-edit-field", cut.Markup);

        // Cancel editing — the Close icon button (second of the two icon buttons in edit mode)
        var closeButtons = cut.FindAll("button[title='Cancel']");
        closeButtons.First().Click();

        Assert.DoesNotContain("todo-edit-field", cut.Markup);
    }

    [Fact]
    public async Task SaveEdit_UpdatesTodoTitle()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Old title");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // Start editing
        cut.Find(".todo-edit-btn").Click();

        // Change value in edit field
        var editInput = cut.Find(".todo-edit-field input");
        editInput.Change("New title");

        // Click save
        cut.Find("button[title='Save']").Click();

        await cut.WaitForAssertionAsync(() =>
            Assert.Contains("New title", cut.Markup));
        Assert.DoesNotContain("Old title", cut.Markup);
    }

    [Fact]
    public async Task SelectModeButton_IsRendered_WhenTodosExist()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Test todo");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("select-mode-btn", cut.Markup);
    }

    [Fact]
    public async Task ClickingSelectModeButton_ShowsSelectionCheckboxes()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Test todo");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".select-mode-btn").Click();

        Assert.Contains("todo-select-checkbox", cut.Markup);
    }

    [Fact]
    public async Task SelectingTodo_ShowsBulkActionBar()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Test todo");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".select-mode-btn").Click();

        var selectCheckbox = cut.Find(".todo-select-checkbox input");
        selectCheckbox.Change(true);

        Assert.Contains("bulk-action-bar", cut.Markup);
    }

    [Fact]
    public async Task BulkDelete_RemovesSelectedTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Todo A");
        await addHandler.HandleAsync("Todo B");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // Enter select mode
        cut.Find(".select-mode-btn").Click();

        // Select the first visible todo (newest = "Todo B", since ORDER BY Id DESC)
        cut.Find(".todo-select-checkbox input").Change(true);

        // Click delete
        cut.Find(".bulk-delete-btn").Click();

        await cut.WaitForAssertionAsync(() =>
            Assert.DoesNotContain("Todo B", cut.Markup));
        Assert.Contains("Todo A", cut.Markup);
    }

    [Fact]
    public async Task BulkComplete_MarksSelectedTodoComplete()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Todo to complete");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".select-mode-btn").Click();
        cut.Find(".todo-select-checkbox input").Change(true);
        cut.Find(".bulk-complete-btn").Click();

        await cut.WaitForAssertionAsync(() =>
            Assert.Contains("line-through", cut.Markup));
    }

    [Fact]
    public async Task CancelSelectMode_HidesCheckboxesAndClearsSelection()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Test todo");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // Enter select mode and select a todo
        cut.Find(".select-mode-btn").Click();
        cut.Find(".todo-select-checkbox input").Change(true);
        Assert.Contains("bulk-action-bar", cut.Markup);

        // Cancel select mode
        cut.Find(".select-mode-btn").Click();

        Assert.DoesNotContain("todo-select-checkbox", cut.Markup);
        Assert.DoesNotContain("bulk-action-bar", cut.Markup);
    }

    [Fact]
    public async Task DeleteTodo_ShowsUndoSnackbar()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Task to delete");

        var ctx = CreateBunitContext(db);
        var snackbarProvider = ctx.Render<MudSnackbarProvider>();
        var cut = RenderHome(ctx);

        cut.Find(".todo-delete-btn").Click();

        await cut.WaitForAssertionAsync(() =>
            Assert.Contains("Undo", snackbarProvider.Markup));
        Assert.Contains("deleted", snackbarProvider.Markup);
    }

    [Fact]
    public async Task DeleteTodo_UndoRestoresTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Restore me");

        var ctx = CreateBunitContext(db);
        var snackbarProvider = ctx.Render<MudSnackbarProvider>();
        var cut = RenderHome(ctx);

        // Delete the todo
        cut.Find(".todo-delete-btn").Click();
        await cut.WaitForAssertionAsync(() =>
            Assert.DoesNotContain("Restore me", cut.Markup));

        // Click Undo in the snackbar
        var undoButton = snackbarProvider.Find(".mud-snackbar-action-button");
        undoButton.Click();

        await cut.WaitForAssertionAsync(() =>
            Assert.Contains("Restore me", cut.Markup));
    }

    [Fact]
    public async Task BulkDelete_ShowsUndoSnackbarWithCount()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Todo A");
        await addHandler.HandleAsync("Todo B");

        var ctx = CreateBunitContext(db);
        var snackbarProvider = ctx.Render<MudSnackbarProvider>();
        var cut = RenderHome(ctx);

        cut.Find(".select-mode-btn").Click();
        // Select all checkboxes
        foreach (var checkbox in cut.FindAll(".todo-select-checkbox input"))
            checkbox.Change(true);

        cut.Find(".bulk-delete-btn").Click();

        await cut.WaitForAssertionAsync(() =>
            Assert.Contains("deleted", snackbarProvider.Markup));
        Assert.Contains("Undo", snackbarProvider.Markup);
    }

    [Fact]
    public async Task BulkComplete_ShowsUndoSnackbar()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Task to complete");

        var ctx = CreateBunitContext(db);
        var snackbarProvider = ctx.Render<MudSnackbarProvider>();
        var cut = RenderHome(ctx);

        cut.Find(".select-mode-btn").Click();
        cut.Find(".todo-select-checkbox input").Change(true);
        cut.Find(".bulk-complete-btn").Click();

        await cut.WaitForAssertionAsync(() =>
            Assert.Contains("marked complete", snackbarProvider.Markup));
        Assert.Contains("Undo", snackbarProvider.Markup);
    }

    // Stats panel tests

    [Fact]
    public async Task StatsPanel_IsRendered_WhenTodosExist()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Some task");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("stats-panel", cut.Markup);
    }

    [Fact]
    public async Task StatsPanel_IsNotRendered_WhenNoTodosExist()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.DoesNotContain("stats-panel", cut.Markup);
    }

    [Fact]
    public async Task StatsPanel_ShowsCorrectTotalAndActiveCounts()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var completeHandler = new CompleteTodoHandler(db);

        await addHandler.HandleAsync("Task 1");
        await addHandler.HandleAsync("Task 2");
        var id3 = await addHandler.HandleAsync("Task 3");
        await completeHandler.HandleAsync(id3);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("stats-total", cut.Markup);
        Assert.Contains("Total: 3", cut.Markup);
        Assert.Contains("Active: 2", cut.Markup);
        Assert.Contains("Completed: 1", cut.Markup);
    }

    [Fact]
    public async Task StatsPanel_ShowsOverdueChip_WhenOverdueTodosExist()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Overdue task", dueDate: DateTime.Today.AddDays(-2));

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("stats-overdue", cut.Markup);
        Assert.Contains("Overdue: 1", cut.Markup);
    }

    [Fact]
    public async Task StatsPanel_NoOverdueChip_WhenNoOverdueTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Future task", dueDate: DateTime.Today.AddDays(5));

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("stats-panel", cut.Markup);
        Assert.DoesNotContain("stats-overdue", cut.Markup);
    }

    [Fact]
    public async Task StatsPanel_ShowsProgressBar()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Task");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("stats-progress", cut.Markup);
        Assert.Contains("stats-percentage", cut.Markup);
    }

    // Keyboard shortcuts tests

    [Fact]
    public async Task KeyboardShortcutsButton_IsRendered()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("keyboard-shortcuts-btn", cut.Markup);
    }

    [Fact]
    public async Task ShortcutsPanel_IsNotVisible_ByDefault()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.DoesNotContain("keyboard-shortcuts-panel", cut.Markup);
    }

    [Fact]
    public async Task ClickingShortcutsButton_ShowsShortcutsPanel()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".keyboard-shortcuts-btn").Click();

        Assert.Contains("keyboard-shortcuts-panel", cut.Markup);
    }

    [Fact]
    public async Task ShortcutsPanel_ShowsNShortcut()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".keyboard-shortcuts-btn").Click();

        Assert.Contains("Focus new todo input", cut.Markup);
    }

    [Fact]
    public async Task ShortcutsPanel_ShowsSlashShortcut()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".keyboard-shortcuts-btn").Click();

        Assert.Contains("Focus search", cut.Markup);
    }

    [Fact]
    public async Task ShortcutsPanel_ShowsQuestionMarkShortcut()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".keyboard-shortcuts-btn").Click();

        Assert.Contains("Toggle this panel", cut.Markup);
    }

    [Fact]
    public async Task ClickingShortcutsButton_WhenPanelVisible_HidesPanel()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // Open panel
        cut.Find(".keyboard-shortcuts-btn").Click();
        Assert.Contains("keyboard-shortcuts-panel", cut.Markup);

        // Close panel
        cut.Find(".keyboard-shortcuts-btn").Click();
        Assert.DoesNotContain("keyboard-shortcuts-panel", cut.Markup);
    }

    [Fact]
    public async Task ToggleShortcutsHelp_JSInvokable_ShowsPanel()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        await cut.InvokeAsync(() => cut.Instance.ToggleShortcutsHelp());

        Assert.Contains("keyboard-shortcuts-panel", cut.Markup);
    }
}
