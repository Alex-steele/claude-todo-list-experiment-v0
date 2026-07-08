using System.Text.Json;
using TodoApp.Features.Lists;
using TodoApp.Features.Todos;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.ColorLabel;
using TodoApp.Features.Todos.Export;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.Import;
using TodoApp.Features.Todos.Tags;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Import;

public class JsonImportHandlerTests
{
    private static string MakeJson(object envelope) =>
        JsonSerializer.Serialize(envelope, new JsonSerializerOptions { WriteIndented = false });

    [Fact]
    public async Task Import_InvalidJson_ThrowsJsonException()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new JsonImportHandler(db);

        await Assert.ThrowsAsync<JsonException>(() => handler.HandleAsync("not json"));
    }

    [Fact]
    public async Task Import_EmptyTodosArray_ReturnsZero()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new JsonImportHandler(db);

        var json = MakeJson(new { Todos = Array.Empty<object>() });
        var result = await handler.HandleAsync(json);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Import_NullTodosArray_ReturnsZero()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new JsonImportHandler(db);

        var json = MakeJson(new { List = "My List", Todos = (object?)null });
        var result = await handler.HandleAsync(json);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Import_SingleActiveTodo_CreatesTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new JsonImportHandler(db);
        var get = new GetTodosHandler(db);

        var json = MakeJson(new
        {
            Todos = new[] { new { Title = "Buy milk", IsCompleted = false, Priority = "none" } }
        });

        var count = await handler.HandleAsync(json);
        Assert.Equal(1, count);

        var todos = await get.HandleAsync();
        Assert.Single(todos, t => t.Title == "Buy milk" && !t.IsCompleted);
    }

    [Fact]
    public async Task Import_CompletedTodo_SetsIsCompleted()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new JsonImportHandler(db);
        var get = new GetTodosHandler(db);

        var json = MakeJson(new
        {
            Todos = new[] { new { Title = "Done thing", IsCompleted = true, Priority = "none" } }
        });

        await handler.HandleAsync(json);

        var todos = await get.HandleAsync();
        Assert.Single(todos, t => t.Title == "Done thing" && t.IsCompleted);
    }

    [Fact]
    public async Task Import_PriorityStrings_MappedCorrectly()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new JsonImportHandler(db);
        var get = new GetTodosHandler(db);

        var json = MakeJson(new
        {
            Todos = new[]
            {
                new { Title = "High task",   IsCompleted = false, Priority = "high"   },
                new { Title = "Medium task", IsCompleted = false, Priority = "medium" },
                new { Title = "Low task",    IsCompleted = false, Priority = "low"    },
                new { Title = "None task",   IsCompleted = false, Priority = "none"   }
            }
        });

        await handler.HandleAsync(json);

        var todos = await get.HandleAsync();
        Assert.Equal(TodoPriority.High,   todos.First(t => t.Title == "High task").Priority);
        Assert.Equal(TodoPriority.Medium, todos.First(t => t.Title == "Medium task").Priority);
        Assert.Equal(TodoPriority.Low,    todos.First(t => t.Title == "Low task").Priority);
        Assert.Equal(TodoPriority.None,   todos.First(t => t.Title == "None task").Priority);
    }

    [Fact]
    public async Task Import_DueDate_Preserved()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new JsonImportHandler(db);
        var get = new GetTodosHandler(db);

        var json = MakeJson(new
        {
            Todos = new[] { new { Title = "Deadline", IsCompleted = false, Priority = "none", DueDate = "2026-08-01" } }
        });

        await handler.HandleAsync(json);

        var todos = await get.HandleAsync();
        var todo = Assert.Single(todos, t => t.Title == "Deadline");
        Assert.Equal(new DateTime(2026, 8, 1), todo.DueDate!.Value.Date);
    }

    [Fact]
    public async Task Import_Tags_ImportedPerTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new JsonImportHandler(db);
        var get = new GetTodosHandler(db);
        var getTags = new GetTodoTagsHandler(db);

        var json = MakeJson(new
        {
            Todos = new[] { new { Title = "Tagged todo", IsCompleted = false, Priority = "none", Tags = new[] { "work", "urgent" } } }
        });

        await handler.HandleAsync(json);

        var todos = await get.HandleAsync();
        var todo = Assert.Single(todos, t => t.Title == "Tagged todo");
        var tagDict = await getTags.HandleAsync(new[] { todo.Id });
        var tags = tagDict[todo.Id];
        Assert.Equal(2, tags.Count);
        Assert.Contains(tags, t => t.Name == "work");
        Assert.Contains(tags, t => t.Name == "urgent");
    }

    [Fact]
    public async Task Import_IsPinned_Preserved()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new JsonImportHandler(db);
        var get = new GetTodosHandler(db);

        var json = MakeJson(new
        {
            Todos = new[] { new { Title = "Pinned todo", IsCompleted = false, Priority = "none", IsPinned = true } }
        });

        await handler.HandleAsync(json);

        var todos = await get.HandleAsync();
        var todo = Assert.Single(todos, t => t.Title == "Pinned todo");
        Assert.True(todo.IsPinned);
    }

    [Fact]
    public async Task Import_Notes_Preserved()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new JsonImportHandler(db);
        var get = new GetTodosHandler(db);

        var json = MakeJson(new
        {
            Todos = new[] { new { Title = "Noted todo", IsCompleted = false, Priority = "none", Notes = "Some detail here" } }
        });

        await handler.HandleAsync(json);

        var todos = await get.HandleAsync();
        var todo = Assert.Single(todos, t => t.Title == "Noted todo");
        Assert.Equal("Some detail here", todo.Notes);
    }

    [Fact]
    public async Task Import_MultipleTodos_AllImported()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new JsonImportHandler(db);
        var get = new GetTodosHandler(db);

        var json = MakeJson(new
        {
            Todos = new[]
            {
                new { Title = "Alpha", IsCompleted = false, Priority = "none" },
                new { Title = "Beta",  IsCompleted = false, Priority = "none" },
                new { Title = "Gamma", IsCompleted = true,  Priority = "none" }
            }
        });

        var count = await handler.HandleAsync(json);
        Assert.Equal(3, count);

        var todos = await get.HandleAsync();
        Assert.Equal(3, todos.Count);
    }

    [Fact]
    public async Task Import_TargetListId_TodosLandInCorrectList()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new JsonImportHandler(db);
        var createList = new CreateListHandler(db);
        var get = new GetTodosHandler(db);

        var workId = await createList.HandleAsync("Work");
        var json = MakeJson(new
        {
            Todos = new[] { new { Title = "Work task", IsCompleted = false, Priority = "none" } }
        });

        await handler.HandleAsync(json, listId: workId);

        var allTodos = await get.HandleAsync();
        var workTodos    = allTodos.Where(t => t.ListId == workId).ToList();
        var personalTodos = allTodos.Where(t => t.ListId == 1).ToList();
        Assert.Single(workTodos, t => t.Title == "Work task");
        Assert.DoesNotContain(personalTodos, t => t.Title == "Work task");
    }

    [Fact]
    public async Task Import_RoundTrip_WithJsonExport()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getHandler = new GetTodosHandler(db);
        var exportHandler = new JsonExportHandler();
        var importHandler = new JsonImportHandler(db);
        var createList = new CreateListHandler(db);

        await addHandler.HandleAsync("Task One");
        await addHandler.HandleAsync("Task Two");
        var original = await getHandler.HandleAsync();

        var json = exportHandler.Generate("Personal", original);
        var destId = await createList.HandleAsync("Imported");
        var count = await importHandler.HandleAsync(json, listId: destId);

        Assert.Equal(2, count);
        var all = await getHandler.HandleAsync();
        var imported = all.Where(t => t.ListId == destId).ToList();
        Assert.Equal(2, imported.Count);
        Assert.Contains(imported, t => t.Title == "Task One");
        Assert.Contains(imported, t => t.Title == "Task Two");
    }

    [Fact]
    public async Task Import_ColorLabel_Preserved()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new JsonImportHandler(db);
        var get = new GetTodosHandler(db);

        var json = MakeJson(new
        {
            Todos = new[] { new { Title = "Colorful", IsCompleted = false, Priority = "none", ColorLabel = "red" } }
        });

        await handler.HandleAsync(json);

        var todos = await get.HandleAsync();
        var todo = Assert.Single(todos, t => t.Title == "Colorful");
        Assert.Equal(TodoColorLabel.Red, todo.ColorLabel);
    }
}
