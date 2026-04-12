using TodoApp.Features.Todos;
using TodoApp.Features.Todos.Export;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.Import;
using TodoApp.Features.Todos.Tags;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Import;

public class ImportTodosHandlerTests
{
    private const string ValidHeader = "Id,Title,Priority,DueDate,IsCompleted,CreatedAt,Tags,Notes";

    [Fact]
    public async Task Import_EmptyContent_ReturnsZero()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new ImportTodosHandler(db);

        var result = await handler.HandleAsync("");

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Import_HeaderOnly_ReturnsZero()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new ImportTodosHandler(db);

        var result = await handler.HandleAsync(ValidHeader);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Import_InvalidHeader_ReturnsZero()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new ImportTodosHandler(db);

        var result = await handler.HandleAsync("NotAValidHeader\n1,Some todo");

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Import_SingleTodo_CreatesIt()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new ImportTodosHandler(db);
        var getTodos = new GetTodosHandler(db);

        var csv = $"{ValidHeader}\n1,Buy milk,None,,False,2026-01-01T00:00:00,,";
        var count = await handler.HandleAsync(csv);

        Assert.Equal(1, count);
        var todos = await getTodos.HandleAsync();
        Assert.Single(todos);
        Assert.Equal("Buy milk", todos[0].Title);
    }

    [Fact]
    public async Task Import_MultipleTodos_AllCreated()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new ImportTodosHandler(db);
        var getTodos = new GetTodosHandler(db);

        var csv = string.Join("\n", [
            ValidHeader,
            "1,Task one,None,,False,2026-01-01T00:00:00,,",
            "2,Task two,None,,False,2026-01-01T00:00:00,,",
            "3,Task three,None,,False,2026-01-01T00:00:00,,"
        ]);

        var count = await handler.HandleAsync(csv);

        Assert.Equal(3, count);
        var todos = await getTodos.HandleAsync();
        Assert.Equal(3, todos.Count);
    }

    [Fact]
    public async Task Import_Priority_ParsedCorrectly()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new ImportTodosHandler(db);
        var getTodos = new GetTodosHandler(db);

        var csv = string.Join("\n", [
            ValidHeader,
            "1,Low task,Low,,False,2026-01-01T00:00:00,,",
            "2,High task,High,,False,2026-01-01T00:00:00,,"
        ]);
        await handler.HandleAsync(csv);

        var todos = await getTodos.HandleAsync();
        Assert.Contains(todos, t => t.Title == "Low task" && t.Priority == TodoPriority.Low);
        Assert.Contains(todos, t => t.Title == "High task" && t.Priority == TodoPriority.High);
    }

    [Fact]
    public async Task Import_DueDate_ParsedCorrectly()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new ImportTodosHandler(db);
        var getTodos = new GetTodosHandler(db);

        var csv = $"{ValidHeader}\n1,Due task,None,2026-06-15,False,2026-01-01T00:00:00,,";
        await handler.HandleAsync(csv);

        var todos = await getTodos.HandleAsync();
        Assert.Equal(new DateTime(2026, 6, 15), todos[0].DueDate?.Date);
    }

    [Fact]
    public async Task Import_IsCompleted_ParsedCorrectly()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new ImportTodosHandler(db);
        var getTodos = new GetTodosHandler(db);

        var csv = string.Join("\n", [
            ValidHeader,
            "1,Done task,None,,True,2026-01-01T00:00:00,,",
            "2,Active task,None,,False,2026-01-01T00:00:00,,"
        ]);
        await handler.HandleAsync(csv);

        var todos = await getTodos.HandleAsync();
        Assert.True(todos.First(t => t.Title == "Done task").IsCompleted);
        Assert.False(todos.First(t => t.Title == "Active task").IsCompleted);
    }

    [Fact]
    public async Task Import_Tags_CreatedCorrectly()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new ImportTodosHandler(db);
        var getTodos = new GetTodosHandler(db);
        var getTags = new GetTodoTagsHandler(db);

        var csv = $"{ValidHeader}\n1,Tagged task,None,,False,2026-01-01T00:00:00,health|urgent,";
        await handler.HandleAsync(csv);

        var todos = await getTodos.HandleAsync();
        var tags = await getTags.HandleAsync([todos[0].Id]);
        var tagNames = tags[todos[0].Id].Select(t => t.Name).ToList();

        Assert.Contains("health", tagNames);
        Assert.Contains("urgent", tagNames);
    }

    [Fact]
    public async Task Import_Notes_SetCorrectly()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new ImportTodosHandler(db);
        var getTodos = new GetTodosHandler(db);

        var csv = $"{ValidHeader}\n1,Task with notes,None,,False,2026-01-01T00:00:00,,Remember to call first";
        await handler.HandleAsync(csv);

        var todos = await getTodos.HandleAsync();
        Assert.Equal("Remember to call first", todos[0].Notes);
    }

    [Fact]
    public async Task Import_TitleWithComma_ParsedCorrectly()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new ImportTodosHandler(db);
        var getTodos = new GetTodosHandler(db);

        var csv = $"{ValidHeader}\n1,\"Buy milk, eggs\",None,,False,2026-01-01T00:00:00,,";
        await handler.HandleAsync(csv);

        var todos = await getTodos.HandleAsync();
        Assert.Equal("Buy milk, eggs", todos[0].Title);
    }

    [Fact]
    public async Task Import_RoundTrip_ExportThenImportRestoresAllTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new TodoApp.Features.Todos.AddTodo.AddTodoHandler(db);
        var addTag = new TodoApp.Features.Todos.Tags.AddTagHandler(db);
        var getTodos = new GetTodosHandler(db);
        var getTags = new GetTodoTagsHandler(db);
        var exportHandler = new CsvExportHandler();
        var importHandler = new ImportTodosHandler(db);

        // Create some todos
        var id1 = await addTodo.HandleAsync("Task one", TodoPriority.High, DateTime.Today.AddDays(3));
        var id2 = await addTodo.HandleAsync("Task two");
        await addTag.HandleAsync(id1, "work");

        var todos = await getTodos.HandleAsync();
        var tags = await getTags.HandleAsync([id1, id2]);

        // Export
        var csv = exportHandler.Generate(todos, tags);

        // Import into a fresh database
        var db2 = await TestDatabase.CreateAsync();
        var importHandler2 = new ImportTodosHandler(db2);
        var getTodos2 = new GetTodosHandler(db2);

        var count = await importHandler2.HandleAsync(csv);

        Assert.Equal(2, count);
        var imported = await getTodos2.HandleAsync();
        Assert.Contains(imported, t => t.Title == "Task one" && t.Priority == TodoPriority.High);
        Assert.Contains(imported, t => t.Title == "Task two");
    }
}
