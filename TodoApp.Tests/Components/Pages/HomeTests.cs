using Bunit;
using TodoApp.Features.Todos;
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
using TodoApp.Features.Todos.ClearCompleted;
using TodoApp.Features.Todos.Export;
using TodoApp.Features.Todos.UpdateNotes;
using TodoApp.Features.Todos.PinTodo;
using TodoApp.Features.Todos.GetTodosStats;
using TodoApp.Features.Todos.Import;
using TodoApp.Features.Todos.Subtasks;
using TodoApp.Features.Todos.Tags;
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
        ctx.Services.AddScoped<ClearCompletedHandler>();
        ctx.Services.AddScoped<CsvExportHandler>();
        ctx.Services.AddScoped<PinTodoHandler>();
        ctx.Services.AddScoped<UpdateNotesHandler>();
        ctx.Services.AddScoped<AddTagHandler>();
        ctx.Services.AddScoped<RemoveTagHandler>();
        ctx.Services.AddScoped<GetTodoTagsHandler>();
        ctx.Services.AddScoped<ImportTodosHandler>();
        ctx.Services.AddScoped<AddSubtaskHandler>();
        ctx.Services.AddScoped<CompleteSubtaskHandler>();
        ctx.Services.AddScoped<DeleteSubtaskHandler>();
        ctx.Services.AddScoped<GetSubtasksHandler>();
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

        // Relative display: 5 days out shows "Due in 5 days"
        Assert.Contains("Due in 5 days", cut.Markup);
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

        // Relative display: 3 days overdue shows "Overdue by 3 days"
        Assert.Contains("Overdue by 3 days", cut.Markup);
        // MudBlazor renders Color.Error text with mud-error-text CSS class
        Assert.Contains("mud-error-text", cut.Markup);
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

    // Tag tests

    [Fact]
    public async Task AddTagButton_IsRendered_ForEachTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Walk the dog");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("add-tag-btn", cut.Markup);
    }

    [Fact]
    public async Task ClickAddTagButton_ShowsTagInput()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Walk the dog");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".add-tag-btn").Click();

        Assert.Contains("todo-tag-input", cut.Markup);
    }

    [Fact]
    public async Task TagsPreAddedToTodo_AreDisplayedAsChips()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addTag = new AddTagHandler(db);

        var todoId = await addTodo.HandleAsync("Walk the dog");
        await addTag.HandleAsync(todoId, "health");
        await addTag.HandleAsync(todoId, "daily");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("todo-tag-chip", cut.Markup);
        Assert.Contains("health", cut.Markup);
        Assert.Contains("daily", cut.Markup);
    }

    [Fact]
    public async Task TodoWithNoTags_ShowsAddTagButton_ButNoChips()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Walk the dog");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.DoesNotContain("todo-tag-chip", cut.Markup);
        Assert.Contains("add-tag-btn", cut.Markup);
    }

    // Notes tests

    [Fact]
    public async Task NotesButton_IsRendered_ForEachTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Walk the dog");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("todo-notes-btn", cut.Markup);
    }

    [Fact]
    public async Task ClickingNotesButton_ShowsNotesEditor()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Walk the dog");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".todo-notes-btn").Click();

        Assert.Contains("todo-notes-editor", cut.Markup);
        Assert.Contains("todo-notes-input", cut.Markup);
    }

    [Fact]
    public async Task ClickingCancelNotes_HidesEditor()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Walk the dog");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".todo-notes-btn").Click();
        Assert.Contains("todo-notes-editor", cut.Markup);

        cut.Find(".todo-notes-cancel-btn").Click();
        Assert.DoesNotContain("todo-notes-editor", cut.Markup);
    }

    [Fact]
    public async Task ExistingNotes_AreDisplayed_InTodoItem()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var updateNotes = new UpdateNotesHandler(db);

        var id = await addTodo.HandleAsync("Walk the dog");
        await updateNotes.HandleAsync(id, "Take the riverside path");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("todo-notes-display", cut.Markup);
        Assert.Contains("Take the riverside path", cut.Markup);
    }

    [Fact]
    public async Task SaveNotes_UpdatesTodoWithNewNotes()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Walk the dog");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".todo-notes-btn").Click();
        var notesInput = cut.Find(".todo-notes-input textarea");
        notesInput.Change("Remember the lead");
        cut.Find(".todo-notes-save-btn").Click();

        await cut.WaitForAssertionAsync(() =>
            Assert.Contains("Remember the lead", cut.Markup));
    }

    // Clear completed tests

    [Fact]
    public async Task ClearCompletedButton_NotRendered_WhenNoCompletedTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Active task");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.DoesNotContain("clear-completed-btn", cut.Markup);
    }

    [Fact]
    public async Task ClearCompletedButton_IsRendered_WhenCompletedTodosExist()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var completeHandler = new CompleteTodoHandler(db);

        var id = await addHandler.HandleAsync("Done task");
        await completeHandler.HandleAsync(id);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("clear-completed-btn", cut.Markup);
    }

    [Fact]
    public async Task ClickingClearCompleted_RemovesCompletedTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var completeHandler = new CompleteTodoHandler(db);

        await addHandler.HandleAsync("Active task");
        var doneId = await addHandler.HandleAsync("Done task");
        await completeHandler.HandleAsync(doneId);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".clear-completed-btn").Click();

        await cut.WaitForAssertionAsync(() =>
            Assert.DoesNotContain("Done task", cut.Markup));
        Assert.Contains("Active task", cut.Markup);
    }

    [Fact]
    public async Task ClearCompleted_ShowsUndoSnackbar()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var completeHandler = new CompleteTodoHandler(db);

        var id = await addHandler.HandleAsync("Done task");
        await completeHandler.HandleAsync(id);

        var ctx = CreateBunitContext(db);
        var snackbarProvider = ctx.Render<MudSnackbarProvider>();
        var cut = RenderHome(ctx);

        cut.Find(".clear-completed-btn").Click();

        await cut.WaitForAssertionAsync(() =>
            Assert.Contains("cleared", snackbarProvider.Markup));
        Assert.Contains("Undo", snackbarProvider.Markup);
    }

    // Pin tests

    [Fact]
    public async Task PinButton_IsRendered_ForEachTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Walk the dog");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("todo-pin-btn", cut.Markup);
    }

    [Fact]
    public async Task PinButton_Title_IsPinToTop_WhenUnpinned()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Walk the dog");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("Pin to top", cut.Markup);
    }

    [Fact]
    public async Task ClickingPinButton_ChangesTitleToUnpin()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Walk the dog");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".todo-pin-btn").Click();

        await cut.WaitForAssertionAsync(() =>
            Assert.Contains("Unpin", cut.Markup));
    }

    [Fact]
    public async Task PinnedTodo_AppearsBeforeUnpinnedTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var pinHandler = new PinTodoHandler(db);

        var id1 = await addTodo.HandleAsync("Older todo");
        var id2 = await addTodo.HandleAsync("Newer todo");   // would normally be first
        await pinHandler.HandleAsync(id1);                   // pin the older one

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        var markup = cut.Markup;
        var olderPos = markup.IndexOf("Older todo", StringComparison.Ordinal);
        var newerPos = markup.IndexOf("Newer todo", StringComparison.Ordinal);

        Assert.True(olderPos < newerPos, "Pinned todo should appear before unpinned todo");
    }

    // Export tests

    [Fact]
    public async Task ExportButton_IsRendered_WhenTodosExist()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Walk the dog");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("export-csv-btn", cut.Markup);
    }

    [Fact]
    public async Task ExportButton_IsNotRendered_WhenNoTodosExist()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.DoesNotContain("export-csv-btn", cut.Markup);
    }

    [Fact]
    public async Task ClickingExportButton_DoesNotThrow()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Walk the dog");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // JSInterop is in Loose mode — JS call completes without error
        var ex = await Record.ExceptionAsync(async () =>
        {
            cut.Find(".export-csv-btn").Click();
            await Task.Delay(50);
        });
        Assert.Null(ex);
    }

    // Tag filter tests

    [Fact]
    public async Task TagFilterRow_IsNotRendered_WhenNoTagsExist()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Walk the dog");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.DoesNotContain("tag-filter-row", cut.Markup);
    }

    [Fact]
    public async Task TagFilterRow_IsRendered_WhenTagsExist()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addTag = new AddTagHandler(db);

        var id = await addTodo.HandleAsync("Walk the dog");
        await addTag.HandleAsync(id, "health");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("tag-filter-row", cut.Markup);
        Assert.Contains("tag-filter-chip", cut.Markup);
        Assert.Contains("health", cut.Markup);
    }

    [Fact]
    public async Task ClickingTagFilter_ShowsOnlyTodosWithThatTag()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addTag = new AddTagHandler(db);

        var id1 = await addTodo.HandleAsync("Walk the dog");
        var id2 = await addTodo.HandleAsync("Buy groceries");
        await addTag.HandleAsync(id1, "health");
        await addTag.HandleAsync(id2, "shopping");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("Walk the dog", cut.Markup);
        Assert.Contains("Buy groceries", cut.Markup);

        // Click the "health" filter chip
        var healthChip = cut.FindAll(".tag-filter-chip").First(c => c.TextContent.Contains("health"));
        healthChip.Click();

        Assert.Contains("Walk the dog", cut.Markup);
        Assert.DoesNotContain("Buy groceries", cut.Markup);
    }

    [Fact]
    public async Task ClickingAllTagFilter_ShowsAllTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addTag = new AddTagHandler(db);

        var id1 = await addTodo.HandleAsync("Walk the dog");
        var id2 = await addTodo.HandleAsync("Buy groceries");
        await addTag.HandleAsync(id1, "health");
        await addTag.HandleAsync(id2, "shopping");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // Filter to "health" first
        var healthChip = cut.FindAll(".tag-filter-chip").First(c => c.TextContent.Contains("health"));
        healthChip.Click();
        Assert.DoesNotContain("Buy groceries", cut.Markup);

        // Click "All" to reset
        cut.Find(".tag-filter-all").Click();

        Assert.Contains("Walk the dog", cut.Markup);
        Assert.Contains("Buy groceries", cut.Markup);
    }

    [Fact]
    public async Task TagFilter_CombinesWithStatusFilter()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addTag = new AddTagHandler(db);
        var complete = new CompleteTodoHandler(db);

        var id1 = await addTodo.HandleAsync("Walk the dog");
        var id2 = await addTodo.HandleAsync("Go for a run");
        await addTag.HandleAsync(id1, "health");
        await addTag.HandleAsync(id2, "health");
        await complete.HandleAsync(id2);   // "Go for a run" is completed

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // Filter by "health" tag
        var healthChip = cut.FindAll(".tag-filter-chip").First(c => c.TextContent.Contains("health"));
        healthChip.Click();

        // Both health todos visible
        Assert.Contains("Walk the dog", cut.Markup);
        Assert.Contains("Go for a run", cut.Markup);

        // Also filter to Active only
        var activeButton = cut.FindAll("button").First(b => b.TextContent.Trim() == "Active");
        activeButton.Click();

        // Only the active health todo should remain
        Assert.Contains("Walk the dog", cut.Markup);
        Assert.DoesNotContain("Go for a run", cut.Markup);
    }

    // Import tests

    [Fact]
    public async Task ImportButton_IsRendered_WhenTodosExist()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Some task");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("import-csv-btn", cut.Markup);
    }

    // Due date UX tests

    [Fact]
    public async Task DueDateShortcuts_AreRendered_InAddForm()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("due-date-shortcuts", cut.Markup);
        Assert.Contains("due-date-today-btn", cut.Markup);
        Assert.Contains("due-date-tomorrow-btn", cut.Markup);
        Assert.Contains("due-date-next-week-btn", cut.Markup);
    }

    [Fact]
    public async Task ClickingTodayShortcut_ThenAddingTodo_ShowsDueTodayInList()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // Set "Today" shortcut
        cut.Find(".due-date-today-btn").Click();

        // Fill in title and submit
        var input = cut.Find(".new-todo-input input");
        input.Change("Urgent task");
        cut.Find(".mud-button-root.mud-button-filled-primary").Click();

        await cut.WaitForAssertionAsync(() =>
            Assert.Contains("Due today", cut.Markup));
    }

    [Fact]
    public async Task ClickingTomorrowShortcut_ThenAddingTodo_ShowsDueTomorrowInList()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".due-date-tomorrow-btn").Click();

        var input = cut.Find(".new-todo-input input");
        input.Change("Task due tomorrow");
        cut.Find(".mud-button-root.mud-button-filled-primary").Click();

        await cut.WaitForAssertionAsync(() =>
            Assert.Contains("Due tomorrow", cut.Markup));
    }

    [Fact]
    public async Task RelativeDueDate_OverdueByOneDay_ShowsCorrectText()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Yesterday's task", dueDate: DateTime.Today.AddDays(-1));

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("Overdue by 1 day", cut.Markup);
    }

    [Fact]
    public async Task RelativeDueDate_FarFuture_ShowsAbsoluteDate()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var farFuture = DateTime.Today.AddDays(30);
        await addHandler.HandleAsync("Long-term task", dueDate: farFuture);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains(farFuture.ToString("MMM d, yyyy"), cut.Markup);
    }

    // Subtask tests

    [Fact]
    public async Task SubtasksToggleButton_IsRendered_ForEachTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Main task");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("subtasks-toggle-btn", cut.Markup);
    }

    [Fact]
    public async Task ClickingSubtasksToggle_ExpandsSubtasksSection()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Main task");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".subtasks-toggle-btn").Click();

        Assert.Contains("add-subtask-btn", cut.Markup);
    }

    [Fact]
    public async Task ExistingSubtasks_AreShown_WhenExpanded()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addSubtask = new AddSubtaskHandler(db);

        var todoId = await addTodo.HandleAsync("Main task");
        await addSubtask.HandleAsync(todoId, "Step one");
        await addSubtask.HandleAsync(todoId, "Step two");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".subtasks-toggle-btn").Click();

        Assert.Contains("Step one", cut.Markup);
        Assert.Contains("Step two", cut.Markup);
    }

    [Fact]
    public async Task SubtasksToggleButton_ShowsProgressCount_WhenSubtasksExist()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addSubtask = new AddSubtaskHandler(db);
        var completeSubtask = new CompleteSubtaskHandler(db);

        var todoId = await addTodo.HandleAsync("Main task");
        await addSubtask.HandleAsync(todoId, "Step one");
        var id2 = await addSubtask.HandleAsync(todoId, "Step two");
        await completeSubtask.HandleAsync(id2);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // Button shows "1/2 subtasks"
        Assert.Contains("1/2", cut.Markup);
    }

    [Fact]
    public async Task ClickingSubtasksToggleTwice_CollapsesSection()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Main task");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".subtasks-toggle-btn").Click();
        Assert.Contains("add-subtask-btn", cut.Markup);

        cut.Find(".subtasks-toggle-btn").Click();
        Assert.DoesNotContain("add-subtask-btn", cut.Markup);
    }

    // Priority filter tests

    [Fact]
    public async Task PriorityFilterRow_IsRendered_WhenTodosExist()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Some task");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("priority-filter-row", cut.Markup);
    }

    [Fact]
    public async Task PriorityFilter_High_ShowsOnlyHighPriorityTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("High task", TodoPriority.High);
        await addHandler.HandleAsync("Low task",  TodoPriority.Low);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".priority-filter-high").Click();

        Assert.Contains("High task", cut.Markup);
        Assert.DoesNotContain("Low task", cut.Markup);
    }

    [Fact]
    public async Task PriorityFilter_All_ShowsAllTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("High task",   TodoPriority.High);
        await addHandler.HandleAsync("Medium task", TodoPriority.Medium);
        await addHandler.HandleAsync("No priority");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // Filter to High first, then reset
        cut.Find(".priority-filter-high").Click();
        cut.Find(".priority-filter-all").Click();

        Assert.Contains("High task", cut.Markup);
        Assert.Contains("Medium task", cut.Markup);
        Assert.Contains("No priority", cut.Markup);
    }

    [Fact]
    public async Task PriorityFilter_NoMatch_ShowsNoMatchMessage()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Low task", TodoPriority.Low);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".priority-filter-high").Click();

        Assert.Contains("No todos match your filters", cut.Markup);
    }
}
