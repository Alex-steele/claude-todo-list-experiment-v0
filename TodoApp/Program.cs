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
builder.Services.AddScoped<ImportTodosHandler>();
builder.Services.AddScoped<PinTodoHandler>();
builder.Services.AddScoped<AddTagHandler>();
builder.Services.AddScoped<RemoveTagHandler>();
builder.Services.AddScoped<GetTodoTagsHandler>();
builder.Services.AddScoped<AddSubtaskHandler>();
builder.Services.AddScoped<CompleteSubtaskHandler>();
builder.Services.AddScoped<DeleteSubtaskHandler>();
builder.Services.AddScoped<GetSubtasksHandler>();
builder.Services.AddScoped<CreateRecurringInstanceHandler>();
builder.Services.AddScoped<GetListsHandler>();
builder.Services.AddScoped<CreateListHandler>();
builder.Services.AddScoped<DeleteListHandler>();
builder.Services.AddScoped<RenameListHandler>();
builder.Services.AddScoped<ReorderTodosHandler>();

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
