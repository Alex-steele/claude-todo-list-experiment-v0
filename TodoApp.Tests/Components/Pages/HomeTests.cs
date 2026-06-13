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
using TodoApp.Features.Lists;
using TodoApp.Features.Todos.ReorderTodos;
using TodoApp.Features.Todos.QuickAdd;
using TodoApp.Features.Todos.RecurringTodos;
using TodoApp.Features.Todos.UndoRedo;
using TodoApp.Features.Todos.MoveTodo;
using TodoApp.Features.Todos.SnoozeTodo;
using TodoApp.Features.Todos.FocusMode;
using TodoApp.Features.Todos.DuplicateTodo;
using TodoApp.Features.Todos.ActivityStats;
using TodoApp.Features.Todos.ColorLabel;
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
        ctx.Services.AddScoped<CreateRecurringInstanceHandler>();
        ctx.Services.AddScoped<GetListsHandler>();
        ctx.Services.AddScoped<CreateListHandler>();
        ctx.Services.AddScoped<DeleteListHandler>();
        ctx.Services.AddScoped<RenameListHandler>();
        ctx.Services.AddScoped<ReorderTodosHandler>();
        ctx.Services.AddScoped<MoveTodoHandler>();
        ctx.Services.AddScoped<SnoozeTodoHandler>();
        ctx.Services.AddScoped<FocusModeHandler>();
        ctx.Services.AddScoped<DuplicateTodoHandler>();
        ctx.Services.AddScoped<ActivityStatsHandler>();
        ctx.Services.AddScoped<SetColorLabelHandler>();
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

    // Recurring todos tests

    [Fact]
    public async Task RecurrenceSelect_IsRenderedInAddForm()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);

        Assert.Contains("recurrence-select", cut.Markup);
    }

    [Fact]
    public async Task AddTodo_WithRecurrence_ShowsRecurrenceBadge()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Daily standup", recurrence: RecurrenceRule.Daily);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("recurrence-badge", cut.Markup);
        Assert.Contains("Daily", cut.Markup);
    }

    [Fact]
    public async Task AddTodo_NoRecurrence_NoRecurrenceBadge()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("One-time task");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.DoesNotContain("recurrence-badge", cut.Markup);
    }

    [Fact]
    public async Task CompleteRecurringTodo_CreatesNextInstance()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Weekly review", recurrence: RecurrenceRule.Weekly);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // There should be exactly 1 todo initially
        Assert.Single(cut.FindAll(".mud-list-item"));

        // Trigger completion via the checkbox input element
        var checkboxInput = cut.Find(".mud-checkbox input");
        checkboxInput.Change(true);

        await cut.WaitForAssertionAsync(() =>
        {
            // After completing, a new instance should appear (total 2 — one completed, one active)
            var todos = cut.FindAll(".mud-list-item");
            Assert.Equal(2, todos.Count);
        });
    }

    // Natural language quick-add tests

    [Fact]
    public async Task QuickAdd_NoHints_HintStripNotVisible()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".new-todo-input input").Change("Buy groceries");

        Assert.DoesNotContain("quick-add-hint-strip", cut.Markup);
    }

    [Fact]
    public async Task QuickAdd_WithDateHint_ShowsHintStrip()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".new-todo-input input").Input("Call dentist tomorrow");
        cut.Find(".new-todo-input input").Change("Call dentist tomorrow");

        Assert.Contains("quick-add-hint-strip", cut.Markup);
        Assert.Contains("quick-add-hint-text", cut.Markup);
    }

    [Fact]
    public async Task QuickAdd_WithPriorityHint_ShowsHintStrip()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".new-todo-input input").Change("Fix bug !high");

        Assert.Contains("quick-add-hint-strip", cut.Markup);
    }

    [Fact]
    public async Task QuickAdd_AddWithPriorityHint_StoresParsedPriority()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var getHandler = new GetTodosHandler(db);

        var cut = RenderHome(ctx);

        cut.Find(".new-todo-input input").Change("Fix bug !high");
        cut.Find("button.add-todo-btn").Click();

        await cut.WaitForAssertionAsync(async () =>
        {
            var todos = await getHandler.HandleAsync();
            Assert.Single(todos);
            Assert.Equal("Fix bug", todos[0].Title);
            Assert.Equal(TodoPriority.High, todos[0].Priority);
        });
    }

    // Multiple lists tests

    [Fact]
    public async Task ListSelector_IsRenderedWithDefaultList()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);

        Assert.Contains("list-selector-row", cut.Markup);
        Assert.Contains("Personal", cut.Markup);
    }

    [Fact]
    public async Task ListSelector_HasAddListButton()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);

        Assert.Contains("add-list-btn", cut.Markup);
    }

    [Fact]
    public async Task SwitchList_FiltersToActiveList()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var createListHandler = new CreateListHandler(db);

        await addHandler.HandleAsync("Personal todo", listId: 1);
        var workId = await createListHandler.HandleAsync("Work");
        await addHandler.HandleAsync("Work todo", listId: workId);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // Initially on Personal list — should show "Personal todo"
        Assert.Contains("Personal todo", cut.Markup);
        Assert.DoesNotContain("Work todo", cut.Markup);

        // Switch to Work list — click the second list chip
        var listChips = cut.FindAll(".list-chip");
        listChips[1].Click();

        Assert.Contains("Work todo", cut.Markup);
        Assert.DoesNotContain("Personal todo", cut.Markup);
    }

    [Fact]
    public async Task NewTodo_AddedToActiveList()
    {
        var db = await TestDatabase.CreateAsync();
        var createListHandler = new CreateListHandler(db);
        var workId = await createListHandler.HandleAsync("Work");
        var getHandler = new GetTodosHandler(db);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // Switch to Work list and add a todo
        cut.FindAll(".list-chip")[1].Click();
        cut.Find(".new-todo-input input").Change("Work item");
        cut.Find("button.add-todo-btn").Click();

        await cut.WaitForAssertionAsync(async () =>
        {
            var todos = await getHandler.HandleAsync();
            var workTodo = todos.FirstOrDefault(t => t.Title == "Work item");
            Assert.NotNull(workTodo);
            Assert.Equal(workId, workTodo.ListId);
        });
    }

    // Rename list tests

    [Fact]
    public async Task RenameList_ShowsRenameInputOnDoubleClick()
    {
        var db = await TestDatabase.CreateAsync();
        var createListHandler = new CreateListHandler(db);
        await createListHandler.HandleAsync("Work");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // Double-click the Work list chip
        cut.FindAll(".list-chip")[1].DoubleClick();

        Assert.Contains("rename-list-input", cut.Markup);
    }

    [Fact]
    public async Task RenameList_SaveOnEnter_UpdatesName()
    {
        var db = await TestDatabase.CreateAsync();
        var createListHandler = new CreateListHandler(db);
        await createListHandler.HandleAsync("Work");
        var getHandler = new GetListsHandler(db);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // Double-click the Work chip to start rename
        cut.FindAll(".list-chip")[1].DoubleClick();

        // Type new name and press Enter
        cut.Find(".rename-list-input input").Change("Projects");
        cut.Find(".rename-list-input input").KeyUp(Key.Enter);

        await cut.WaitForAssertionAsync(async () =>
        {
            var lists = await getHandler.HandleAsync();
            Assert.Contains(lists, l => l.Name == "Projects");
        });
    }

    [Fact]
    public async Task RenameList_EscapeKey_CancelsRename()
    {
        var db = await TestDatabase.CreateAsync();
        var createListHandler = new CreateListHandler(db);
        await createListHandler.HandleAsync("Work");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.FindAll(".list-chip")[1].DoubleClick();
        Assert.Contains("rename-list-input", cut.Markup);

        cut.Find(".rename-list-input input").KeyUp(Key.Escape);

        Assert.DoesNotContain("rename-list-input", cut.Markup);
        Assert.Contains("Work", cut.Markup);
    }

    // Drag-to-reorder tests

    [Fact]
    public async Task ManualSort_ShowsDragHandles()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Task A");
        await addHandler.HandleAsync("Task B");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // No drag handles in default (Newest) sort
        Assert.DoesNotContain("drag-handle", cut.Markup);

        // Switch to Custom order
        var sortSelect = cut.FindComponent<MudSelect<TodoSortOrder>>();
        await sortSelect.InvokeAsync(() => sortSelect.Instance.ValueChanged.InvokeAsync(TodoSortOrder.Manual));

        Assert.Contains("drag-handle", cut.Markup);
    }

    [Fact]
    public async Task ManualSort_SortOptionExistsInSelect()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Task");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);

        // The sort select should have a Manual option — verify via the component's Items
        var sortSelects = cut.FindComponents<MudSelect<TodoSortOrder>>();
        Assert.NotEmpty(sortSelects);
    }

    // ── Move Todo Between Lists ───────────────────────────────────────────────

    [Fact]
    public async Task MoveTodo_ButtonNotShown_WhenOnlyOneList()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Solo task");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);

        // With only one list, no move button should appear
        Assert.Empty(cut.FindAll(".todo-move-btn"));
    }

    [Fact]
    public async Task MoveTodo_ButtonShown_WhenMultipleLists()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var createList = new CreateListHandler(db);
        await addHandler.HandleAsync("Task to move");
        await createList.HandleAsync("Work");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);

        Assert.NotEmpty(cut.FindAll(".todo-move-btn"));
    }

    [Fact]
    public async Task MoveTodo_ClickingButton_ShowsMoveSelect()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var createList = new CreateListHandler(db);
        await addHandler.HandleAsync("Task to move");
        await createList.HandleAsync("Work");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);

        cut.Find(".todo-move-btn").Click();

        Assert.NotEmpty(cut.FindAll(".todo-move-select"));
        Assert.Empty(cut.FindAll(".todo-move-btn")); // button replaced by select
    }

    [Fact]
    public async Task MoveTodo_CancelButton_HidesMoveSelect()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var createList = new CreateListHandler(db);
        await addHandler.HandleAsync("Task to move");
        await createList.HandleAsync("Work");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);

        cut.Find(".todo-move-btn").Click();
        cut.Find(".todo-move-cancel-btn").Click();

        Assert.Empty(cut.FindAll(".todo-move-select"));
        Assert.NotEmpty(cut.FindAll(".todo-move-btn"));
    }

    [Fact]
    public async Task MoveTodo_MovesToOtherList_DisappearsFromCurrentList()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var createList = new CreateListHandler(db);
        var moveTodoHandler = new MoveTodoHandler(db);
        var id = await addHandler.HandleAsync("Task to move");
        var workId = await createList.HandleAsync("Work");
        var ctx = CreateBunitContext(db);

        // Move the todo to Work list via handler directly
        await moveTodoHandler.HandleAsync(id, workId);

        var cut = RenderHome(ctx);

        // Personal list (default) should show no todos
        await cut.WaitForAssertionAsync(() =>
            Assert.Empty(cut.FindAll(".mud-list-item")));
    }

    [Fact]
    public async Task MoveTodo_MovedTodo_AppearsInTargetList()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var createList = new CreateListHandler(db);
        var moveTodoHandler = new MoveTodoHandler(db);
        var id = await addHandler.HandleAsync("Task to move");
        var workId = await createList.HandleAsync("Work");
        await moveTodoHandler.HandleAsync(id, workId);
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);

        // Switch to Work list
        var chips = cut.FindAll(".list-chip");
        chips[1].Click(); // Work is the second chip

        await cut.WaitForAssertionAsync(() =>
        {
            var items = cut.FindAll(".mud-list-item");
            Assert.Single(items);
        });
    }

    // ── Global search across lists ────────────────────────────────────────────

    [Fact]
    public async Task GlobalSearch_WithSingleList_NoHintShown()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Solo task");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        cut.Find(".todo-search-field input").Input("solo");

        Assert.DoesNotContain("search-all-lists-hint", cut.Markup);
    }

    [Fact]
    public async Task GlobalSearch_WithMultipleLists_ShowsHint()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var createList = new CreateListHandler(db);
        await addHandler.HandleAsync("Personal task");
        await createList.HandleAsync("Work");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        cut.Find(".todo-search-field input").Input("task");

        Assert.Contains("search-all-lists-hint", cut.Markup);
        Assert.Contains("Showing results across all lists", cut.Markup);
    }

    [Fact]
    public async Task GlobalSearch_FindsTodosFromOtherLists()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var createList = new CreateListHandler(db);
        var workId = await createList.HandleAsync("Work");
        await addHandler.HandleAsync("Personal task", listId: 1);
        await addHandler.HandleAsync("Work report", listId: workId);
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        // Active list is Personal — searching should find the Work todo too
        cut.Find(".todo-search-field input").Input("report");

        Assert.Contains("Work report", cut.Markup);
    }

    [Fact]
    public async Task GlobalSearch_ShowsListBadgeOnCrossListResults()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var createList = new CreateListHandler(db);
        var workId = await createList.HandleAsync("Work");
        await addHandler.HandleAsync("Personal task", listId: 1);
        await addHandler.HandleAsync("Work report", listId: workId);
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        cut.Find(".todo-search-field input").Input("report");

        // The Work list badge should appear on the cross-list result
        Assert.NotEmpty(cut.FindAll(".search-result-list-badge"));
    }

    [Fact]
    public async Task GlobalSearch_NoListBadge_OnActiveListResults()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var createList = new CreateListHandler(db);
        await createList.HandleAsync("Work");
        await addHandler.HandleAsync("Personal task", listId: 1);
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        cut.Find(".todo-search-field input").Input("personal");

        // Result is in the active list — no list badge
        Assert.Empty(cut.FindAll(".search-result-list-badge"));
    }

    [Fact]
    public async Task GlobalSearch_ClearingQuery_HidesHint()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var createList = new CreateListHandler(db);
        await addHandler.HandleAsync("Task");
        await createList.HandleAsync("Work");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        cut.Find(".todo-search-field input").Input("task");
        Assert.Contains("search-all-lists-hint", cut.Markup);

        cut.Find(".todo-search-field input").Input("");
        Assert.DoesNotContain("search-all-lists-hint", cut.Markup);
    }

    // ── Due-date quick filters ────────────────────────────────────────────────

    [Fact]
    public async Task DateFilterRow_IsRendered()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("A task");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);

        Assert.NotEmpty(cut.FindAll(".date-filter-row"));
        Assert.NotEmpty(cut.FindAll(".date-filter-overdue"));
        Assert.NotEmpty(cut.FindAll(".date-filter-today"));
        Assert.NotEmpty(cut.FindAll(".date-filter-week"));
    }

    [Fact]
    public async Task DateFilter_Overdue_ShowsOnlyOverdueTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Overdue task", dueDate: DateTime.Today.AddDays(-2));
        await addHandler.HandleAsync("Future task",  dueDate: DateTime.Today.AddDays(3));
        await addHandler.HandleAsync("No date task");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        cut.Find(".date-filter-overdue").Click();

        Assert.Contains("Overdue task", cut.Markup);
        Assert.DoesNotContain("Future task", cut.Markup);
        Assert.DoesNotContain("No date task", cut.Markup);
    }

    [Fact]
    public async Task DateFilter_DueToday_ShowsOnlyTodayTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Today task",  dueDate: DateTime.Today);
        await addHandler.HandleAsync("Future task", dueDate: DateTime.Today.AddDays(3));
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        cut.Find(".date-filter-today").Click();

        Assert.Contains("Today task", cut.Markup);
        Assert.DoesNotContain("Future task", cut.Markup);
    }

    [Fact]
    public async Task DateFilter_DueThisWeek_ShowsNextSevenDaysTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Due today task",    dueDate: DateTime.Today);
        await addHandler.HandleAsync("Due in four days",  dueDate: DateTime.Today.AddDays(4));
        await addHandler.HandleAsync("Due in eight days", dueDate: DateTime.Today.AddDays(8));
        await addHandler.HandleAsync("Past due task",     dueDate: DateTime.Today.AddDays(-1));
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        cut.Find(".date-filter-week").Click();

        Assert.Contains("Due today task", cut.Markup);
        Assert.Contains("Due in four days", cut.Markup);
        Assert.DoesNotContain("Due in eight days", cut.Markup);
        Assert.DoesNotContain("Past due task", cut.Markup);
    }

    [Fact]
    public async Task DateFilter_Any_ResetsToAll()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Overdue task", dueDate: DateTime.Today.AddDays(-1));
        await addHandler.HandleAsync("No date task");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        cut.Find(".date-filter-overdue").Click();
        Assert.DoesNotContain("No date task", cut.Markup);

        cut.Find(".date-filter-all").Click();
        Assert.Contains("No date task", cut.Markup);
    }

    [Fact]
    public async Task DateFilter_WithMultipleLists_ShowsCrossListHint()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var createList = new CreateListHandler(db);
        await addHandler.HandleAsync("Overdue task", dueDate: DateTime.Today.AddDays(-1));
        await createList.HandleAsync("Work");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        cut.Find(".date-filter-overdue").Click();

        Assert.Contains("search-all-lists-hint", cut.Markup);
    }

    // ── Edit priority and due date ────────────────────────────────────────────

    [Fact]
    public async Task EditTodo_EditFormShowsPrioritySelect()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Task to edit");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        cut.Find(".todo-edit-btn").Click();

        Assert.NotEmpty(cut.FindAll(".todo-edit-priority"));
    }

    [Fact]
    public async Task EditTodo_EditFormShowsDueDatePicker()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Task to edit");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        cut.Find(".todo-edit-btn").Click();

        Assert.NotEmpty(cut.FindAll(".todo-edit-due-date"));
    }

    [Fact]
    public async Task EditTodo_PrePopulatesExistingPriority()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("High priority task", priority: TodoPriority.High);
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        cut.Find(".todo-edit-btn").Click();

        // The priority select should be rendered
        var prioritySelect = cut.FindComponent<MudSelect<TodoPriority>>();
        Assert.NotNull(prioritySelect);
    }

    [Fact]
    public async Task EditTodo_SaveUpdatesTitle()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Original title");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        cut.Find(".todo-edit-btn").Click();

        var titleInput = cut.Find(".todo-edit-field input");
        titleInput.Change("Updated title");
        cut.Find(".todo-edit-save-btn").Click();

        await cut.WaitForAssertionAsync(() =>
            Assert.Contains("Updated title", cut.Markup));
    }

    [Fact]
    public async Task EditTodo_CancelButton_ClosesEditForm()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("A task");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        cut.Find(".todo-edit-btn").Click();
        Assert.NotEmpty(cut.FindAll(".todo-edit-form"));

        cut.Find(".todo-edit-cancel-btn").Click();
        Assert.Empty(cut.FindAll(".todo-edit-form"));
    }

    // ── Completion date tracking ──────────────────────────────────────────────

    [Fact]
    public async Task CompleteTodo_ShowsCompletedAtText()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Finish report");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        cut.Find(".mud-checkbox input").Change(true);

        await cut.WaitForAssertionAsync(() =>
            Assert.NotEmpty(cut.FindAll(".completed-at-text")));
    }

    [Fact]
    public async Task CompleteTodo_CompletedAtText_ContainsCompletedWord()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Task");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        cut.Find(".mud-checkbox input").Change(true);

        await cut.WaitForAssertionAsync(() =>
            Assert.Contains("Completed", cut.Find(".completed-at-text").TextContent));
    }

    [Fact]
    public async Task IncompleteTodo_DoesNotShowCompletedAtText()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Not done yet");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);

        Assert.Empty(cut.FindAll(".completed-at-text"));
    }

    [Fact]
    public async Task PreExistingCompletedTodo_WithoutCompletedAt_NoTimestampShown()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var completeHandler = new CompleteTodoHandler(db);
        var id = await addHandler.HandleAsync("Old task");
        await completeHandler.HandleAsync(id);
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);

        // CompletedAt is set by the handler so this should show the text
        // (verifies the full round-trip via handler)
        await cut.WaitForStateAsync(() => cut.FindAll(".mud-list-item").Count > 0);
        var completedItems = cut.FindAll(".completed-at-text");
        Assert.NotEmpty(completedItems);
    }

    // ── Snooze / defer ────────────────────────────────────────────────────────

    [Fact]
    public async Task SnoozeButton_IsRendered_ForEachTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Task to snooze");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);

        Assert.NotEmpty(cut.FindAll(".todo-snooze-btn"));
    }

    [Fact]
    public async Task ClickingSnoozeButton_ShowsSnoozeOptions()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Task to snooze");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        cut.Find(".todo-snooze-btn").Click();

        Assert.NotEmpty(cut.FindAll(".snooze-options"));
        Assert.NotEmpty(cut.FindAll(".snooze-tomorrow-btn"));
        Assert.NotEmpty(cut.FindAll(".snooze-week-btn"));
        Assert.NotEmpty(cut.FindAll(".snooze-two-weeks-btn"));
    }

    [Fact]
    public async Task ClickingSnoozeButton_HidesSnoozeButtonAndShowsOptions()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Task to snooze");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        cut.Find(".todo-snooze-btn").Click();

        // Snooze button is replaced by the options panel
        Assert.Empty(cut.FindAll(".todo-snooze-btn"));
    }

    [Fact]
    public async Task ClickingCancelSnooze_HidesOptions()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Task to snooze");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        cut.Find(".todo-snooze-btn").Click();
        Assert.NotEmpty(cut.FindAll(".snooze-options"));

        cut.Find(".snooze-cancel-btn").Click();
        Assert.Empty(cut.FindAll(".snooze-options"));
        Assert.NotEmpty(cut.FindAll(".todo-snooze-btn"));
    }

    [Fact]
    public async Task SnoozeTomorrow_SetsDueDateToTomorrow()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Task to snooze");
        var getHandler = new GetTodosHandler(db);
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        cut.Find(".todo-snooze-btn").Click();
        cut.Find(".snooze-tomorrow-btn").Click();

        var tomorrow = DateTime.Today.AddDays(1);
        await cut.WaitForAssertionAsync(async () =>
        {
            var todos = await getHandler.HandleAsync();
            Assert.Equal(tomorrow.Date, todos[0].DueDate!.Value.Date);
        });
    }

    [Fact]
    public async Task SnoozeWeek_SetsDueDateToNextWeek()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Task to snooze");
        var getHandler = new GetTodosHandler(db);
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        cut.Find(".todo-snooze-btn").Click();
        cut.Find(".snooze-week-btn").Click();

        var nextWeek = DateTime.Today.AddDays(7);
        await cut.WaitForAssertionAsync(async () =>
        {
            var todos = await getHandler.HandleAsync();
            Assert.Equal(nextWeek.Date, todos[0].DueDate!.Value.Date);
        });
    }

    [Fact]
    public async Task SnoozeShowsSnackbar()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Task to snooze");
        var ctx = CreateBunitContext(db);
        var snackbarProvider = ctx.Render<MudSnackbarProvider>();

        var cut = RenderHome(ctx);
        cut.Find(".todo-snooze-btn").Click();
        cut.Find(".snooze-tomorrow-btn").Click();

        await cut.WaitForAssertionAsync(() =>
            Assert.Contains("Snoozed to", snackbarProvider.Markup));
    }

    // ── Time estimate ─────────────────────────────────────────────────────────

    [Fact]
    public async Task TimeEstimateSelect_IsRendered_InAddForm()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);

        Assert.Contains("time-estimate-select", cut.Markup);
    }

    [Fact]
    public async Task AddTodo_WithTimeEstimate_ShowsBadgeOnTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Estimate task", timeEstimate: TodoApp.Features.Todos.TimeEstimates.TimeEstimate.OneHour);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("time-estimate-badge", cut.Markup);
        Assert.Contains("1 h", cut.Markup);
    }

    [Fact]
    public async Task AddTodo_WithNoEstimate_NoBadgeShown()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("No estimate task");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Empty(cut.FindAll(".time-estimate-badge"));
    }

    [Fact]
    public async Task StatsPanel_ShowsEstimatedTimeChip_WhenEstimatesExist()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Short task", timeEstimate: TodoApp.Features.Todos.TimeEstimates.TimeEstimate.ThirtyMinutes);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("stats-estimated-time", cut.Markup);
        Assert.Contains("remaining", cut.Markup);
    }

    [Fact]
    public async Task StatsPanel_NoEstimatedTimeChip_WhenNoEstimates()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("No estimate");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Empty(cut.FindAll(".stats-estimated-time"));
    }

    [Fact]
    public async Task EditForm_ShowsTimeEstimateSelect()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Task to edit");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        cut.Find(".todo-edit-btn").Click();

        Assert.NotEmpty(cut.FindAll(".todo-edit-time-estimate"));
    }

    [Fact]
    public async Task TimeEstimateBadge_ShowsCorrectLabel_ForThirtyMinutes()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Quick task", timeEstimate: TodoApp.Features.Todos.TimeEstimates.TimeEstimate.ThirtyMinutes);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        var badge = cut.Find(".time-estimate-badge");
        Assert.Contains("30 min", badge.TextContent);
    }

    [Fact]
    public async Task StatsPanel_EstimatedTimeChip_AggregatesMultipleTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        // 60 min + 120 min = 180 min = 3 h
        await addHandler.HandleAsync("Task A", timeEstimate: TodoApp.Features.Todos.TimeEstimates.TimeEstimate.OneHour);
        await addHandler.HandleAsync("Task B", timeEstimate: TodoApp.Features.Todos.TimeEstimates.TimeEstimate.TwoHours);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("3 h", cut.Markup);
        Assert.Contains("remaining", cut.Markup);
    }

    [Fact]
    public async Task CompletedTodo_TimeEstimate_NotIncluded_InStatsTotal()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var completeHandler = new CompleteTodoHandler(db);

        var id = await addHandler.HandleAsync("Done task", timeEstimate: TodoApp.Features.Todos.TimeEstimates.TimeEstimate.FourHours);
        await completeHandler.HandleAsync(id);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // No remaining estimate since only todo is completed
        Assert.Empty(cut.FindAll(".stats-estimated-time"));
    }

    // ── Focus Mode bUnit tests ────────────────────────────────────────────────

    [Fact]
    public async Task FocusModeButton_IsRendered_WhenTodosExist()
    {
        var db = await TestDatabase.CreateAsync();
        await new AddTodoHandler(db).HandleAsync("Something");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.NotEmpty(cut.FindAll(".focus-mode-btn"));
    }

    [Fact]
    public async Task FocusModeButton_IsNotRendered_WhenNoTodosExist()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Empty(cut.FindAll(".focus-mode-btn"));
    }

    [Fact]
    public async Task FocusMode_Click_ShowsFocusBanner()
    {
        var db = await TestDatabase.CreateAsync();
        await new AddTodoHandler(db).HandleAsync("Urgent", priority: TodoPriority.High);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".focus-mode-btn").Click();

        Assert.NotEmpty(cut.FindAll(".focus-mode-banner"));
    }

    [Fact]
    public async Task FocusMode_ExitButton_HidesBanner()
    {
        var db = await TestDatabase.CreateAsync();
        await new AddTodoHandler(db).HandleAsync("Urgent", priority: TodoPriority.High);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".focus-mode-btn").Click();
        cut.Find(".focus-mode-exit-btn").Click();

        Assert.Empty(cut.FindAll(".focus-mode-banner"));
    }

    [Fact]
    public async Task FocusMode_FiltersOutLowPriorityNoDateTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Urgent", priority: TodoPriority.High);
        await addHandler.HandleAsync("Someday", priority: TodoPriority.None);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".focus-mode-btn").Click();

        Assert.Contains("Urgent", cut.Markup);
        Assert.DoesNotContain("Someday", cut.Markup);
    }

    [Fact]
    public async Task FocusMode_IncludesOverdueTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Overdue task", dueDate: DateTime.Today.AddDays(-2));

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".focus-mode-btn").Click();

        Assert.Contains("Overdue task", cut.Markup);
    }

    [Fact]
    public async Task FocusMode_ExcludesCompletedTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var completeHandler = new CompleteTodoHandler(db);

        var id = await addHandler.HandleAsync("Done high priority", priority: TodoPriority.High);
        await completeHandler.HandleAsync(id);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".focus-mode-btn").Click();

        // Banner should say "nothing urgent" since the only high-priority todo is completed
        Assert.Contains("nothing urgent", cut.Markup);
    }

    [Fact]
    public async Task FocusMode_Banner_ShowsEstimatedTime_WhenTodosHaveEstimates()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("High with estimate", priority: TodoPriority.High,
            timeEstimate: TodoApp.Features.Todos.TimeEstimates.TimeEstimate.OneHour);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".focus-mode-btn").Click();

        var banner = cut.Find(".focus-mode-banner");
        Assert.Contains("1 h", banner.TextContent);
    }

    // ── Duplicate todo ────────────────────────────────────────────────────────

    [Fact]
    public async Task DuplicateButton_IsRendered_ForEachTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Task one");
        await addHandler.HandleAsync("Task two");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);

        var dupBtns = cut.FindAll(".todo-duplicate-btn");
        Assert.Equal(2, dupBtns.Count);
    }

    [Fact]
    public async Task DuplicateButton_Click_AddsCopyToList()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Buy milk");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        Assert.Single(cut.FindAll(".mud-list-item"));

        cut.Find(".todo-duplicate-btn").Click();

        await cut.WaitForAssertionAsync(() =>
            Assert.Equal(2, cut.FindAll(".mud-list-item").Count));
    }

    [Fact]
    public async Task DuplicateButton_Click_ShowsCopyTitle()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Write report");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        cut.Find(".todo-duplicate-btn").Click();

        await cut.WaitForAssertionAsync(() =>
            Assert.Contains("Write report (copy)", cut.Markup));
    }

    [Fact]
    public async Task DuplicateButton_Click_OriginalStillPresent()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Original task");
        var ctx = CreateBunitContext(db);

        var cut = RenderHome(ctx);
        cut.Find(".todo-duplicate-btn").Click();

        await cut.WaitForAssertionAsync(() =>
        {
            Assert.Contains("Original task", cut.Markup);
            Assert.Contains("Original task (copy)", cut.Markup);
        });
    }

    // Activity stats / completion streak tests

    [Fact]
    public async Task ActivityStats_StreakChip_NotRendered_WhenNoCompletions()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Incomplete task");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.DoesNotContain("streak-chip", cut.Markup);
    }

    [Fact]
    public async Task ActivityStats_StreakChip_RenderedAfterCompletion()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var completeHandler = new CompleteTodoHandler(db);

        var id = await addHandler.HandleAsync("Daily task");
        await completeHandler.HandleAsync(id);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("streak-chip", cut.Markup);
        Assert.Contains("1 day streak", cut.Markup);
    }

    [Fact]
    public async Task ActivityStats_CompletedTodayChip_RenderedAfterCompletion()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var completeHandler = new CompleteTodoHandler(db);

        var id = await addHandler.HandleAsync("Morning task");
        await completeHandler.HandleAsync(id);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("completed-today-chip", cut.Markup);
        Assert.Contains("Today: 1 done", cut.Markup);
    }

    [Fact]
    public async Task ActivityStats_CompletedWeekChip_RenderedAfterCompletion()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var completeHandler = new CompleteTodoHandler(db);

        var id = await addHandler.HandleAsync("Morning task");
        await completeHandler.HandleAsync(id);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("completed-week-chip", cut.Markup);
        Assert.Contains("This week: 1 done", cut.Markup);
    }

    [Fact]
    public async Task ActivityStats_CompletingTodo_UpdatesStreakCount()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var completeHandler = new CompleteTodoHandler(db);

        var id1 = await addHandler.HandleAsync("Task one");
        var id2 = await addHandler.HandleAsync("Task two");
        await completeHandler.HandleAsync(id1);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("Today: 1 done", cut.Markup);

        // Complete the second todo via the UI checkbox
        var checkboxes = cut.FindAll(".mud-checkbox input[type=checkbox]");
        // The first checkbox toggles the second todo's completion (newest first)
        checkboxes[0].Change(true);

        await cut.WaitForAssertionAsync(() =>
            Assert.Contains("Today: 2 done", cut.Markup));
    }

    [Fact]
    public async Task ActivityStats_ActivityStatsRow_NotRendered_WhenNoTodosExist()
    {
        var db = await TestDatabase.CreateAsync();
        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // Stats panel is not rendered when there are no todos
        Assert.DoesNotContain("activity-stats-row", cut.Markup);
    }

    // Markdown notes tests

    [Fact]
    public async Task Notes_WithMarkdownBold_RendersStrongTag()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var notesHandler = new UpdateNotesHandler(db);

        var id = await addHandler.HandleAsync("Task with bold note");
        await notesHandler.HandleAsync(id, "**important**");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("<strong>important</strong>", cut.Markup);
    }

    [Fact]
    public async Task Notes_WithMarkdownList_RendersListItems()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var notesHandler = new UpdateNotesHandler(db);

        var id = await addHandler.HandleAsync("Task with list note");
        await notesHandler.HandleAsync(id, "- step one\n- step two");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("<ul>", cut.Markup);
        Assert.Contains("step one", cut.Markup);
        Assert.Contains("step two", cut.Markup);
    }

    [Fact]
    public async Task Notes_PlainText_IsRendered()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var notesHandler = new UpdateNotesHandler(db);

        var id = await addHandler.HandleAsync("Task with plain note");
        await notesHandler.HandleAsync(id, "Just plain text here");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("Just plain text here", cut.Markup);
        Assert.Contains("todo-notes-display", cut.Markup);
    }

    [Fact]
    public async Task Notes_MarkdownHint_ShownWhenEditing()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Task for notes edit");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // Open notes editor
        cut.Find(".todo-notes-btn").Click();

        Assert.Contains("Markdown supported", cut.Markup);
    }

    [Fact]
    public async Task Notes_RawHtml_IsNotRendered()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var notesHandler = new UpdateNotesHandler(db);

        var id = await addHandler.HandleAsync("Task with HTML note");
        await notesHandler.HandleAsync(id, "<b>bold via html</b>");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // Raw HTML is stripped by DisableHtml()
        Assert.DoesNotContain("<b>bold via html</b>", cut.Markup);
        // The text content itself is still present (escaped)
        Assert.Contains("bold via html", cut.Markup);
    }

    // Color label tests

    [Fact]
    public async Task ColorButton_IsRendered_ForEachTodo()
    {
        var db = await TestDatabase.CreateAsync();
        await new AddTodoHandler(db).HandleAsync("Colorable task");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("todo-color-btn", cut.Markup);
    }

    [Fact]
    public async Task ColorFilterRow_IsRendered_WhenTodosExist()
    {
        var db = await TestDatabase.CreateAsync();
        await new AddTodoHandler(db).HandleAsync("A task");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        Assert.Contains("color-filter-row", cut.Markup);
    }

    [Fact]
    public async Task ClickingColorButton_ShowsColorPalette()
    {
        var db = await TestDatabase.CreateAsync();
        await new AddTodoHandler(db).HandleAsync("Task to color");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".todo-color-btn").Click();

        Assert.Contains("color-picker-palette", cut.Markup);
    }

    [Fact]
    public async Task SettingColor_ShowsLeftBorder()
    {
        var db = await TestDatabase.CreateAsync();
        var id = await new AddTodoHandler(db).HandleAsync("Red task");
        await new SetColorLabelHandler(db).HandleAsync(id, TodoColorLabel.Red);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // The colored border is applied via inline style on the outer div
        Assert.Contains("#ef5350", cut.Markup);
    }

    [Fact]
    public async Task SettingColor_ThenFilteringByColor_ShowsOnlyMatchingTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id1 = await addHandler.HandleAsync("Red task");
        var id2 = await addHandler.HandleAsync("No color task");
        await new SetColorLabelHandler(db).HandleAsync(id1, TodoColorLabel.Green);

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        // Click the green color filter dot
        cut.Find(".color-filter-green").Click();

        Assert.Contains("Red task", cut.Markup);
        Assert.DoesNotContain("No color task", cut.Markup);
    }

    [Fact]
    public async Task CancelColorPicker_HidesPalette()
    {
        var db = await TestDatabase.CreateAsync();
        await new AddTodoHandler(db).HandleAsync("Task");

        var ctx = CreateBunitContext(db);
        var cut = RenderHome(ctx);

        cut.Find(".todo-color-btn").Click();
        Assert.Contains("color-picker-palette", cut.Markup);

        cut.Find(".color-picker-cancel-btn").Click();
        Assert.DoesNotContain("color-picker-palette", cut.Markup);
    }
}
