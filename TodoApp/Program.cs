using MudBlazor.Services;
using TodoApp.Components;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.BulkOperations;
using TodoApp.Features.Todos.CompleteTodo;
using TodoApp.Features.Todos.DeleteTodo;
using TodoApp.Features.Todos.EditTodo;
using TodoApp.Features.Todos.FilterSortTodos;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.GetTodosStats;
using TodoApp.Features.Todos.ClearCompleted;
using TodoApp.Features.Todos.Export;
using TodoApp.Features.Todos.UpdateNotes;
using TodoApp.Features.Todos.PinTodo;
using TodoApp.Features.Todos.Import;
using TodoApp.Features.Todos.Subtasks;
using TodoApp.Features.Todos.Tags;
using TodoApp.Features.Lists;
using TodoApp.Features.Todos.ReorderTodos;
using TodoApp.Features.Todos.RecurringTodos;
using TodoApp.Features.Todos.UndoRedo;
using TodoApp.Features.Todos.MoveTodo;
using TodoApp.Features.Todos.SnoozeTodo;
using TodoApp.Features.Todos.FocusMode;
using TodoApp.Features.Todos.DuplicateTodo;
using TodoApp.Features.Todos.ActivityStats;
using TodoApp.Features.Todos.ColorLabel;
using TodoApp.Features.Todos.SetDueDate;
using TodoApp.Features.FilterPresets;
using TodoApp.Features.Todos.MultiAdd;
using TodoApp.Features.Todos.Templates;
using TodoApp.Features.Goals;
using TodoApp.Features.Todos.WeeklySummary;
using TodoApp.Features.Todos.RandomPicker;
using TodoApp.Features.Todos.DueSummary;
using TodoApp.Features.Todos.FilterCounts;
using TodoApp.Features.Todos.StreakNudge;
using TodoApp.Features.Todos.CompletionTimeAnalytics;
using TodoApp.Features.Todos.PriorityBreakdown;
using TodoApp.Features.Todos.BlockTodo;
using TodoApp.Features.Todos.TagStats;
using TodoApp.Features.Todos.TodayView;
using TodoApp.Features.Todos.Links;
using TodoApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

var dbPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "TodoApp",
    "todos.db");
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

var database = new Database($"Data Source={dbPath}");
await database.InitializeAsync();
builder.Services.AddSingleton(database);

builder.Services.AddScoped<AddTodoHandler>();
builder.Services.AddScoped<CompleteTodoHandler>();
builder.Services.AddScoped<DeleteTodoHandler>();
builder.Services.AddScoped<EditTodoHandler>();
builder.Services.AddScoped<GetTodosHandler>();
builder.Services.AddScoped<FilterSortTodosHandler>();
builder.Services.AddScoped<BulkOperationsHandler>();
builder.Services.AddScoped<RestoreTodosHandler>();
builder.Services.AddScoped<GetTodosStatsHandler>();
builder.Services.AddScoped<ClearCompletedHandler>();
builder.Services.AddScoped<UpdateNotesHandler>();
builder.Services.AddScoped<CsvExportHandler>();
builder.Services.AddScoped<MarkdownExportHandler>();
builder.Services.AddScoped<ImportTodosHandler>();
builder.Services.AddScoped<PinTodoHandler>();
builder.Services.AddScoped<AddTagHandler>();
builder.Services.AddScoped<RemoveTagHandler>();
builder.Services.AddScoped<RenameTagHandler>();
builder.Services.AddScoped<GetTodoTagsHandler>();
builder.Services.AddScoped<GetAllTagNamesHandler>();
builder.Services.AddScoped<AddSubtaskHandler>();
builder.Services.AddScoped<EditSubtaskHandler>();
builder.Services.AddScoped<CompleteSubtaskHandler>();
builder.Services.AddScoped<DeleteSubtaskHandler>();
builder.Services.AddScoped<GetSubtasksHandler>();
builder.Services.AddScoped<CreateRecurringInstanceHandler>();
builder.Services.AddScoped<GetListsHandler>();
builder.Services.AddScoped<CreateListHandler>();
builder.Services.AddScoped<DeleteListHandler>();
builder.Services.AddScoped<RenameListHandler>();
builder.Services.AddScoped<ReorderListsHandler>();
builder.Services.AddScoped<ReorderTodosHandler>();
builder.Services.AddScoped<MoveTodoHandler>();
builder.Services.AddScoped<SnoozeTodoHandler>();
builder.Services.AddScoped<FocusModeHandler>();
builder.Services.AddScoped<DuplicateTodoHandler>();
builder.Services.AddScoped<ActivityStatsHandler>();
builder.Services.AddScoped<SetColorLabelHandler>();
builder.Services.AddScoped<SetDueDateHandler>();
builder.Services.AddScoped<SaveFilterPresetHandler>();
builder.Services.AddScoped<GetFilterPresetsHandler>();
builder.Services.AddScoped<DeleteFilterPresetHandler>();
builder.Services.AddScoped<AddMultipleTodosHandler>();
builder.Services.AddScoped<GetTemplatesHandler>();
builder.Services.AddScoped<SaveTemplateHandler>();
builder.Services.AddScoped<DeleteTemplateHandler>();
builder.Services.AddScoped<GetDailyGoalHandler>();
builder.Services.AddScoped<SetDailyGoalHandler>();
builder.Services.AddScoped<GenerateWeeklySummaryHandler>();
builder.Services.AddScoped<PickRandomTodoHandler>();
builder.Services.AddScoped<DueSummaryHandler>();
builder.Services.AddScoped<FilterCountsHandler>();
builder.Services.AddScoped<StreakNudgeHandler>();
builder.Services.AddScoped<CompletionTimeAnalyticsHandler>();
builder.Services.AddScoped<PriorityBreakdownHandler>();
builder.Services.AddScoped<BlockTodoHandler>();
builder.Services.AddScoped<TodayViewHandler>();
builder.Services.AddScoped<TagStatsHandler>();
builder.Services.AddScoped<MarkdownImportHandler>();
builder.Services.AddScoped<JsonImportHandler>();
builder.Services.AddScoped<JsonExportHandler>();
builder.Services.AddScoped<ArchiveListHandler>();
builder.Services.AddScoped<UnarchiveListHandler>();
builder.Services.AddScoped<GetArchivedListsHandler>();
builder.Services.AddScoped<SetTodoUrlHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
