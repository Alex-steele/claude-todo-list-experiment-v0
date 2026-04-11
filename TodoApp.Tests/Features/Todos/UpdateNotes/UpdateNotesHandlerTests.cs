using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.UpdateNotes;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.UpdateNotes;

public class UpdateNotesHandlerTests
{
    [Fact]
    public async Task UpdateNotes_SavesNotesToTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new UpdateNotesHandler(db);
        var get = new GetTodosHandler(db);

        var id = await add.HandleAsync("Walk the dog");
        await handler.HandleAsync(id, "Take the long route around the park");

        var todos = await get.HandleAsync();
        Assert.Equal("Take the long route around the park", todos.Single().Notes);
    }

    [Fact]
    public async Task UpdateNotes_TrimsWhitespace()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new UpdateNotesHandler(db);
        var get = new GetTodosHandler(db);

        var id = await add.HandleAsync("Walk the dog");
        await handler.HandleAsync(id, "  Some notes  ");

        var todos = await get.HandleAsync();
        Assert.Equal("Some notes", todos.Single().Notes);
    }

    [Fact]
    public async Task UpdateNotes_NullOrWhitespace_StoresNull()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new UpdateNotesHandler(db);
        var get = new GetTodosHandler(db);

        var id = await add.HandleAsync("Walk the dog");
        await handler.HandleAsync(id, "Existing notes");
        await handler.HandleAsync(id, "   "); // clear with whitespace

        var todos = await get.HandleAsync();
        Assert.Null(todos.Single().Notes);
    }

    [Fact]
    public async Task UpdateNotes_CanUpdateExistingNotes()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new UpdateNotesHandler(db);
        var get = new GetTodosHandler(db);

        var id = await add.HandleAsync("Walk the dog");
        await handler.HandleAsync(id, "First note");
        await handler.HandleAsync(id, "Updated note");

        var todos = await get.HandleAsync();
        Assert.Equal("Updated note", todos.Single().Notes);
    }

    [Fact]
    public async Task UpdateNotes_NonExistentId_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new UpdateNotesHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(99999, "Some notes"));
    }

    [Fact]
    public async Task UpdateNotes_OnlyAffectsTargetTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new UpdateNotesHandler(db);
        var get = new GetTodosHandler(db);

        var id1 = await add.HandleAsync("Todo A");
        var id2 = await add.HandleAsync("Todo B");
        await handler.HandleAsync(id1, "Notes for A");

        var todos = await get.HandleAsync();
        Assert.Equal("Notes for A", todos.Single(t => t.Id == id1).Notes);
        Assert.Null(todos.Single(t => t.Id == id2).Notes);
    }

    [Fact]
    public async Task NewTodo_DefaultsToNullNotes()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var get = new GetTodosHandler(db);

        await add.HandleAsync("Walk the dog");

        var todos = await get.HandleAsync();
        Assert.Null(todos.Single().Notes);
    }
}
