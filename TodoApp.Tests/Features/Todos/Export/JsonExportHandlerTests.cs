using System.Text.Json;
using TodoApp.Features.Todos;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.ColorLabel;
using TodoApp.Features.Todos.CompleteTodo;
using TodoApp.Features.Todos.Export;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.Tags;
using TodoApp.Features.Todos.TimeEstimates;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Export;

public class JsonExportHandlerTests
{
    [Fact]
    public async Task Generate_EmptyList_ReturnsValidJson()
    {
        var handler = new JsonExportHandler();

        var json = handler.Generate("My List", []);

        var doc = JsonDocument.Parse(json);
        Assert.Equal("My List", doc.RootElement.GetProperty("List").GetString());
        Assert.Equal(0, doc.RootElement.GetProperty("Count").GetProperty("Total").GetInt32());
        Assert.Equal(0, doc.RootElement.GetProperty("Todos").GetArrayLength());
    }

    [Fact]
    public async Task Generate_IncludesListNameAndDate()
    {
        var handler = new JsonExportHandler();

        var json = handler.Generate("Work Tasks", []);

        var doc = JsonDocument.Parse(json);
        Assert.Equal("Work Tasks", doc.RootElement.GetProperty("List").GetString());
        Assert.Equal(DateTime.Today.ToString("yyyy-MM-dd"), doc.RootElement.GetProperty("Exported").GetString());
    }

    [Fact]
    public async Task Generate_CountSummary_IsCorrect()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var complete = new CompleteTodoHandler(db);
        var get = new GetTodosHandler(db);

        var id1 = await add.HandleAsync("Active one");
        var id2 = await add.HandleAsync("Active two");
        var id3 = await add.HandleAsync("Done");
        await complete.HandleAsync(id3);

        var todos = await get.HandleAsync();
        var handler = new JsonExportHandler();

        var json = handler.Generate("List", todos);
        var doc = JsonDocument.Parse(json);
        var count = doc.RootElement.GetProperty("Count");

        Assert.Equal(3, count.GetProperty("Total").GetInt32());
        Assert.Equal(2, count.GetProperty("Active").GetInt32());
        Assert.Equal(1, count.GetProperty("Completed").GetInt32());
    }

    [Fact]
    public async Task Generate_TodoFields_AreSerializedCorrectly()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var get = new GetTodosHandler(db);

        var dueDate = new DateTime(2026, 8, 15);
        await add.HandleAsync("Write report", TodoPriority.High, dueDate);

        var todos = await get.HandleAsync();
        var handler = new JsonExportHandler();

        var json = handler.Generate("My List", todos);
        var doc = JsonDocument.Parse(json);
        var todo = doc.RootElement.GetProperty("Todos")[0];

        Assert.Equal("Write report", todo.GetProperty("Title").GetString());
        Assert.False(todo.GetProperty("IsCompleted").GetBoolean());
        Assert.Equal("high", todo.GetProperty("Priority").GetString());
        Assert.Equal("2026-08-15", todo.GetProperty("DueDate").GetString());
    }

    [Fact]
    public async Task Generate_CompletedTodo_HasCompletedAtField()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var complete = new CompleteTodoHandler(db);
        var get = new GetTodosHandler(db);

        var id = await add.HandleAsync("Finished task");
        await complete.HandleAsync(id);

        var todos = await get.HandleAsync();
        var handler = new JsonExportHandler();

        var json = handler.Generate("List", todos);
        var doc = JsonDocument.Parse(json);
        var todo = doc.RootElement.GetProperty("Todos")[0];

        Assert.True(todo.GetProperty("IsCompleted").GetBoolean());
        Assert.True(todo.TryGetProperty("CompletedAt", out _));
    }

    [Fact]
    public async Task Generate_Tags_IncludedInOutput()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var addTag = new AddTagHandler(db);
        var get = new GetTodosHandler(db);
        var getTags = new GetTodoTagsHandler(db);

        var id = await add.HandleAsync("Tagged task");
        await addTag.HandleAsync(id, "work");
        await addTag.HandleAsync(id, "urgent");

        var todos = await get.HandleAsync();
        var tags = await getTags.HandleAsync([id]);
        var handler = new JsonExportHandler();

        var json = handler.Generate("List", todos, tags);
        var doc = JsonDocument.Parse(json);
        var todoTags = doc.RootElement.GetProperty("Todos")[0].GetProperty("Tags");
        var tagNames = todoTags.EnumerateArray().Select(t => t.GetString()).ToList();

        Assert.Contains("work", tagNames);
        Assert.Contains("urgent", tagNames);
    }

    [Fact]
    public async Task Generate_NoPriority_OutputsNone()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var get = new GetTodosHandler(db);

        await add.HandleAsync("Plain task");

        var todos = await get.HandleAsync();
        var json = new JsonExportHandler().Generate("List", todos);
        var doc = JsonDocument.Parse(json);

        Assert.Equal("none", doc.RootElement.GetProperty("Todos")[0].GetProperty("Priority").GetString());
    }

    [Fact]
    public async Task Generate_NullableFields_OmittedWhenNull()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var get = new GetTodosHandler(db);

        await add.HandleAsync("Simple task");

        var todos = await get.HandleAsync();
        var json = new JsonExportHandler().Generate("List", todos);
        var doc = JsonDocument.Parse(json);
        var todo = doc.RootElement.GetProperty("Todos")[0];

        // DueDate, CompletedAt, Notes, TimeEstimate, ColorLabel should be omitted when null/None
        Assert.False(todo.TryGetProperty("DueDate", out _));
        Assert.False(todo.TryGetProperty("CompletedAt", out _));
        Assert.False(todo.TryGetProperty("Notes", out _));
        Assert.False(todo.TryGetProperty("TimeEstimate", out _));
        Assert.False(todo.TryGetProperty("ColorLabel", out _));
    }

    [Fact]
    public async Task Generate_OutputIsValidJson()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var get = new GetTodosHandler(db);

        await add.HandleAsync("Task one", TodoPriority.Medium, DateTime.Today.AddDays(1));
        await add.HandleAsync("Task two");

        var todos = await get.HandleAsync();
        var json = new JsonExportHandler().Generate("My List", todos);

        // Should not throw
        var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind);
    }

    [Fact]
    public async Task Generate_IsPinned_SerializedCorrectly()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var pin = new TodoApp.Features.Todos.PinTodo.PinTodoHandler(db);
        var get = new GetTodosHandler(db);

        var id = await add.HandleAsync("Pinned task");
        await pin.HandleAsync(id);

        var todos = await get.HandleAsync();
        var json = new JsonExportHandler().Generate("List", todos);
        var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.GetProperty("Todos")[0].GetProperty("IsPinned").GetBoolean());
    }

    [Fact]
    public async Task Generate_IsBlocked_SerializedCorrectly()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var block = new TodoApp.Features.Todos.BlockTodo.BlockTodoHandler(db);
        var get = new GetTodosHandler(db);

        var id = await add.HandleAsync("Blocked task");
        await block.HandleAsync(id);

        var todos = await get.HandleAsync();
        var json = new JsonExportHandler().Generate("List", todos);
        var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.GetProperty("Todos")[0].GetProperty("IsBlocked").GetBoolean());
    }
}
