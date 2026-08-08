using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.Dependencies;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Dependencies;

public class SetDependencyHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidDependency_SetsDependsOnTodoId()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var setHandler = new SetDependencyHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id1 = await addHandler.HandleAsync("Blocked todo");
        var id2 = await addHandler.HandleAsync("Blocking todo");
        await setHandler.HandleAsync(id1, id2);

        var todos = await getHandler.HandleAsync();
        Assert.Equal(id2, todos.Single(t => t.Id == id1).DependsOnTodoId);
    }

    [Fact]
    public async Task HandleAsync_NullDependency_ClearsExistingDependency()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var setHandler = new SetDependencyHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id1 = await addHandler.HandleAsync("Blocked todo");
        var id2 = await addHandler.HandleAsync("Blocking todo");
        await setHandler.HandleAsync(id1, id2);
        await setHandler.HandleAsync(id1, null);

        var todos = await getHandler.HandleAsync();
        Assert.Null(todos.Single(t => t.Id == id1).DependsOnTodoId);
    }

    [Fact]
    public async Task HandleAsync_SelfDependency_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var setHandler = new SetDependencyHandler(db);

        var id = await addHandler.HandleAsync("Todo");

        await Assert.ThrowsAsync<ArgumentException>(() => setHandler.HandleAsync(id, id));
    }

    [Fact]
    public async Task HandleAsync_DirectCycle_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var setHandler = new SetDependencyHandler(db);

        var id1 = await addHandler.HandleAsync("Todo A");
        var id2 = await addHandler.HandleAsync("Todo B");
        await setHandler.HandleAsync(id1, id2);

        await Assert.ThrowsAsync<ArgumentException>(() => setHandler.HandleAsync(id2, id1));
    }

    [Fact]
    public async Task HandleAsync_TransitiveCycle_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var setHandler = new SetDependencyHandler(db);

        var idA = await addHandler.HandleAsync("Todo A");
        var idB = await addHandler.HandleAsync("Todo B");
        var idC = await addHandler.HandleAsync("Todo C");
        await setHandler.HandleAsync(idA, idB); // A -> B
        await setHandler.HandleAsync(idB, idC); // B -> C

        // C -> A would close the loop A -> B -> C -> A
        await Assert.ThrowsAsync<ArgumentException>(() => setHandler.HandleAsync(idC, idA));
    }

    [Fact]
    public async Task HandleAsync_NonExistentDependencyTarget_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var setHandler = new SetDependencyHandler(db);

        var id = await addHandler.HandleAsync("Todo");

        await Assert.ThrowsAsync<ArgumentException>(() => setHandler.HandleAsync(id, 999));
    }

    [Fact]
    public async Task HandleAsync_NonExistentTodoId_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var setHandler = new SetDependencyHandler(db);

        var id = await addHandler.HandleAsync("Todo");

        await Assert.ThrowsAsync<ArgumentException>(() => setHandler.HandleAsync(999, id));
    }
}
