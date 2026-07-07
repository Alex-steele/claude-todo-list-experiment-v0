using TodoApp.Features.Todos;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.Export;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.Import;
using TodoApp.Features.Todos.Tags;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Import;

public class MarkdownImportHandlerTests
{
    [Fact]
    public async Task Import_EmptyContent_ReturnsZero()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new MarkdownImportHandler(db);

        var result = await handler.HandleAsync("");

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Import_NoTodoLines_ReturnsZero()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new MarkdownImportHandler(db);

        var content = "# My List\n\n## Active\n\nSome text without todo lines.";
        var result = await handler.HandleAsync(content);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Import_SingleActiveTodo_CreatesIt()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new MarkdownImportHandler(db);
        var getTodos = new GetTodosHandler(db);

        var result = await handler.HandleAsync("- [ ] Buy milk");

        Assert.Equal(1, result);
        var todos = await getTodos.HandleAsync();
        Assert.Single(todos);
        Assert.Equal("Buy milk", todos[0].Title);
        Assert.False(todos[0].IsCompleted);
    }

    [Fact]
    public async Task Import_SingleCompletedTodo_CreatesAsCompleted()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new MarkdownImportHandler(db);
        var getTodos = new GetTodosHandler(db);

        await handler.HandleAsync("- [x] ~~Done task~~");

        var todos = await getTodos.HandleAsync();
        Assert.Single(todos);
        Assert.Equal("Done task", todos[0].Title);
        Assert.True(todos[0].IsCompleted);
    }

    [Fact]
    public async Task Import_TitleWithMetadata_ExtractsTitle()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new MarkdownImportHandler(db);
        var getTodos = new GetTodosHandler(db);

        await handler.HandleAsync("- [ ] Buy groceries _(high · due 2026-07-10)_");

        var todos = await getTodos.HandleAsync();
        Assert.Equal("Buy groceries", todos[0].Title);
    }

    [Fact]
    public async Task Import_Priority_ParsedCorrectly()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new MarkdownImportHandler(db);
        var getTodos = new GetTodosHandler(db);

        var content = string.Join("\n", [
            "- [ ] Low task _(low)_",
            "- [ ] Medium task _(medium)_",
            "- [ ] High task _(high)_",
        ]);
        await handler.HandleAsync(content);

        var todos = await getTodos.HandleAsync();
        Assert.Contains(todos, t => t.Title == "Low task" && t.Priority == TodoPriority.Low);
        Assert.Contains(todos, t => t.Title == "Medium task" && t.Priority == TodoPriority.Medium);
        Assert.Contains(todos, t => t.Title == "High task" && t.Priority == TodoPriority.High);
    }

    [Fact]
    public async Task Import_DueDate_ParsedCorrectly()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new MarkdownImportHandler(db);
        var getTodos = new GetTodosHandler(db);

        await handler.HandleAsync("- [ ] Meeting _(due 2026-07-10)_");

        var todos = await getTodos.HandleAsync();
        Assert.Equal(new DateTime(2026, 7, 10), todos[0].DueDate?.Date);
    }

    [Fact]
    public async Task Import_OverdueDate_ParsedCorrectly()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new MarkdownImportHandler(db);
        var getTodos = new GetTodosHandler(db);

        await handler.HandleAsync("- [ ] Overdue task _(overdue 2026-06-01)_");

        var todos = await getTodos.HandleAsync();
        Assert.Equal(new DateTime(2026, 6, 1), todos[0].DueDate?.Date);
    }

    [Fact]
    public async Task Import_DueToday_SetsTodayAsDate()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new MarkdownImportHandler(db);
        var getTodos = new GetTodosHandler(db);

        await handler.HandleAsync("- [ ] Urgent task _(due today)_");

        var todos = await getTodos.HandleAsync();
        Assert.Equal(DateTime.Today, todos[0].DueDate?.Date);
    }

    [Fact]
    public async Task Import_Tags_CreatedCorrectly()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new MarkdownImportHandler(db);
        var getTodos = new GetTodosHandler(db);
        var getTags = new GetTodoTagsHandler(db);

        await handler.HandleAsync("- [ ] Tagged task _(#work · #urgent)_");

        var todos = await getTodos.HandleAsync();
        var tags = await getTags.HandleAsync([todos[0].Id]);
        var tagNames = tags[todos[0].Id].Select(t => t.Name).ToList();

        Assert.Contains("work", tagNames);
        Assert.Contains("urgent", tagNames);
    }

    [Fact]
    public async Task Import_MultipleTodos_AllCreated()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new MarkdownImportHandler(db);
        var getTodos = new GetTodosHandler(db);

        var content = string.Join("\n", [
            "- [ ] Task one",
            "- [ ] Task two",
            "- [x] ~~Task three~~",
        ]);
        var count = await handler.HandleAsync(content);

        Assert.Equal(3, count);
        var todos = await getTodos.HandleAsync();
        Assert.Equal(3, todos.Count);
    }

    [Fact]
    public async Task Import_IgnoresNonTodoLines()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new MarkdownImportHandler(db);
        var getTodos = new GetTodosHandler(db);

        var content = """
            # My List

            _Exported 2026-07-07_

            ## Active

            - [ ] Real todo

            ## Completed
            """;
        var count = await handler.HandleAsync(content);

        Assert.Equal(1, count);
        var todos = await getTodos.HandleAsync();
        Assert.Equal("Real todo", todos[0].Title);
    }

    [Fact]
    public async Task Import_ListId_TodoAddedToCorrectList()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new MarkdownImportHandler(db);
        var getTodos = new GetTodosHandler(db);

        await handler.HandleAsync("- [ ] List 2 task", listId: 2);

        var todos = await getTodos.HandleAsync();
        Assert.Single(todos);
        Assert.Equal(2, todos[0].ListId);
    }

    [Fact]
    public async Task Import_StrikethroughWithoutCompletedMark_StillImports()
    {
        // Edge case: someone manually writes ~~title~~ on an active line
        var db = await TestDatabase.CreateAsync();
        var handler = new MarkdownImportHandler(db);
        var getTodos = new GetTodosHandler(db);

        await handler.HandleAsync("- [ ] ~~Strikethrough active~~");

        var todos = await getTodos.HandleAsync();
        Assert.Equal("Strikethrough active", todos[0].Title);
        Assert.False(todos[0].IsCompleted);
    }

    [Fact]
    public async Task Import_ComplexMetadata_ParsedCorrectly()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new MarkdownImportHandler(db);
        var getTodos = new GetTodosHandler(db);
        var getTags = new GetTodoTagsHandler(db);

        await handler.HandleAsync("- [ ] Deploy service _(high · due 2026-07-15 · #work · #devops)_");

        var todos = await getTodos.HandleAsync();
        var todo = todos[0];
        Assert.Equal("Deploy service", todo.Title);
        Assert.Equal(TodoPriority.High, todo.Priority);
        Assert.Equal(new DateTime(2026, 7, 15), todo.DueDate?.Date);

        var tags = await getTags.HandleAsync([todo.Id]);
        var tagNames = tags[todo.Id].Select(t => t.Name).ToList();
        Assert.Contains("work", tagNames);
        Assert.Contains("devops", tagNames);
    }

    [Fact]
    public async Task Import_RoundTrip_MarkdownExportThenImportRestoresTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var addTag = new AddTagHandler(db);
        var getTodos = new GetTodosHandler(db);
        var getTags = new GetTodoTagsHandler(db);
        var exportHandler = new MarkdownExportHandler();
        var importHandler = new MarkdownImportHandler(db);

        // Create todos
        var id1 = await addTodo.HandleAsync("Deploy server", TodoPriority.High, DateTime.Today.AddDays(3));
        var id2 = await addTodo.HandleAsync("Write docs", TodoPriority.Low);
        await addTag.HandleAsync(id1, "work");

        var todos = await getTodos.HandleAsync();
        var tags = await getTags.HandleAsync([id1, id2]);

        // Export to markdown
        var markdown = exportHandler.Generate("My List", todos, tags);

        // Import into a fresh database
        var db2 = await TestDatabase.CreateAsync();
        var importHandler2 = new MarkdownImportHandler(db2);
        var getTodos2 = new GetTodosHandler(db2);
        var getTags2 = new GetTodoTagsHandler(db2);

        var count = await importHandler2.HandleAsync(markdown);

        Assert.Equal(2, count);
        var imported = await getTodos2.HandleAsync();
        var deployTodo = Assert.Single(imported, t => t.Title == "Deploy server");
        Assert.Equal(TodoPriority.High, deployTodo.Priority);

        var importedTags = await getTags2.HandleAsync([deployTodo.Id]);
        var tagNames = importedTags[deployTodo.Id].Select(t => t.Name).ToList();
        Assert.Contains("work", tagNames);

        Assert.Contains(imported, t => t.Title == "Write docs" && t.Priority == TodoPriority.Low);
    }
}
