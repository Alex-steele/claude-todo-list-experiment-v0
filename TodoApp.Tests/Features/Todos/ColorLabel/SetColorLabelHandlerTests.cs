using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.ColorLabel;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.ColorLabel;

public class SetColorLabelHandlerTests
{
    [Fact]
    public async Task HandleAsync_SetsColorOnTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var id = await new AddTodoHandler(db).HandleAsync("Colorful task");

        await new SetColorLabelHandler(db).HandleAsync(id, TodoColorLabel.Red);

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Equal(TodoColorLabel.Red, todos.Single(t => t.Id == id).ColorLabel);
    }

    [Fact]
    public async Task HandleAsync_DefaultColorLabel_IsNone()
    {
        var db = await TestDatabase.CreateAsync();
        var id = await new AddTodoHandler(db).HandleAsync("Plain task");

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Equal(TodoColorLabel.None, todos.Single(t => t.Id == id).ColorLabel);
    }

    [Fact]
    public async Task HandleAsync_CanChangeColorToNone()
    {
        var db = await TestDatabase.CreateAsync();
        var id = await new AddTodoHandler(db).HandleAsync("Task");
        var handler = new SetColorLabelHandler(db);

        await handler.HandleAsync(id, TodoColorLabel.Blue);
        await handler.HandleAsync(id, TodoColorLabel.None);

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Equal(TodoColorLabel.None, todos.Single(t => t.Id == id).ColorLabel);
    }

    [Theory]
    [InlineData(TodoColorLabel.Red)]
    [InlineData(TodoColorLabel.Orange)]
    [InlineData(TodoColorLabel.Yellow)]
    [InlineData(TodoColorLabel.Green)]
    [InlineData(TodoColorLabel.Blue)]
    [InlineData(TodoColorLabel.Purple)]
    public async Task HandleAsync_AllColors_CanBeSet(TodoColorLabel label)
    {
        var db = await TestDatabase.CreateAsync();
        var id = await new AddTodoHandler(db).HandleAsync("Task");

        await new SetColorLabelHandler(db).HandleAsync(id, label);

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Equal(label, todos.Single(t => t.Id == id).ColorLabel);
    }

    [Fact]
    public async Task HandleAsync_InvalidId_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new SetColorLabelHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.HandleAsync(99999, TodoColorLabel.Green));
    }
}
