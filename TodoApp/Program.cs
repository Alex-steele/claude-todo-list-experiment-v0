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
using TodoApp.Features.Todos.Tags;
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
builder.Services.AddScoped<AddTagHandler>();
builder.Services.AddScoped<RemoveTagHandler>();
builder.Services.AddScoped<GetTodoTagsHandler>();

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
