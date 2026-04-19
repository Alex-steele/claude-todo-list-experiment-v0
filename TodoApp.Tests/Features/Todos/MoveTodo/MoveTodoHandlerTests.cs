using TodoApp.Features.Lists;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.MoveTodo;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.MoveTodo;

public class MoveTodoHandlerTests
{
    [Fact]
    public async Task MoveTodo_ChangesListId()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var createList = new CreateListHandler(db);
        var moveTodo = new MoveTodoHandler(db);
        var getTodos = new GetTodosHandler(db);

        var listBId = await createList.HandleAsync("Work");
        var todoId = await addTodo.HandleAsync("Finish report");

        await moveTodo.HandleAsync(todoId, listBId);

        var todos = await getTodos.HandleAsync();
        var moved = todos.Single(t => t.Id == todoId);
        Assert.Equal(listBId, moved.ListId);
    }

    [Fact]
    public async Task MoveTodo_OriginalListNoLongerContainsTodo()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var createList = new CreateListHandler(db);
        var moveTodo = new MoveTodoHandler(db);
        var getTodos = new GetTodosHandler(db);

        var workListId = await createList.HandleAsync("Work");
        var todoId = await addTodo.HandleAsync("Design review", listId: 1);

        await moveTodo.HandleAsync(todoId, workListId);

        var todos = await getTodos.HandleAsync();
        var personalTodos = todos.Where(t => t.ListId == 1).ToList();
        Assert.DoesNotContain(personalTodos, t => t.Id == todoId);
    }

    [Fact]
    public async Task MoveTodo_OtherTodosUnaffected()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var createList = new CreateListHandler(db);
        var moveTodo = new MoveTodoHandler(db);
        var getTodos = new GetTodosHandler(db);

        var workListId = await createList.HandleAsync("Work");
        var todoId1 = await addTodo.HandleAsync("Stay here");
        var todoId2 = await addTodo.HandleAsync("Move me");

        await moveTodo.HandleAsync(todoId2, workListId);

        var todos = await getTodos.HandleAsync();
        var stayed = todos.Single(t => t.Id == todoId1);
        Assert.Equal(1, stayed.ListId);
    }

    [Fact]
    public async Task MoveTodo_ToSameList_IsIdempotent()
    {
        var db = await TestDatabase.CreateAsync();
        var addTodo = new AddTodoHandler(db);
        var moveTodo = new MoveTodoHandler(db);
        var getTodos = new GetTodosHandler(db);

        var todoId = await addTodo.HandleAsync("Same list");

        await moveTodo.HandleAsync(todoId, 1);

        var todos = await getTodos.HandleAsync();
        var todo = todos.Single(t => t.Id == todoId);
        Assert.Equal(1, todo.ListId);
    }
}
