using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.CompleteTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.RescheduleTodos;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.RescheduleTodos;

public class RescheduleOverdueTodosHandlerTests
{
    [Fact]
    public async Task RescheduleOverdue_MovesOverdueTodosToTargetDate()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new RescheduleOverdueTodosHandler(db);
        var get = new GetTodosHandler(db);

        var id = await add.HandleAsync("Overdue task", dueDate: DateTime.Today.AddDays(-3));
        var target = DateTime.Today.AddDays(1);

        var count = await handler.HandleAsync(target, listId: 1);

        Assert.Equal(1, count);
        var todos = await get.HandleAsync();
        Assert.Equal(target.Date, todos.Single(t => t.Id == id).DueDate!.Value.Date);
    }

    [Fact]
    public async Task RescheduleOverdue_ReturnsCountOfRescheduledTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new RescheduleOverdueTodosHandler(db);

        await add.HandleAsync("Overdue 1", dueDate: DateTime.Today.AddDays(-1));
        await add.HandleAsync("Overdue 2", dueDate: DateTime.Today.AddDays(-5));
        await add.HandleAsync("Overdue 3", dueDate: DateTime.Today.AddDays(-10));

        var count = await handler.HandleAsync(DateTime.Today, listId: 1);

        Assert.Equal(3, count);
    }

    [Fact]
    public async Task RescheduleOverdue_DoesNotAffectCompletedTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var complete = new CompleteTodoHandler(db);
        var handler = new RescheduleOverdueTodosHandler(db);
        var get = new GetTodosHandler(db);

        var overdueDate = DateTime.Today.AddDays(-2);
        var id = await add.HandleAsync("Done task", dueDate: overdueDate);
        await complete.HandleAsync(id);

        var count = await handler.HandleAsync(DateTime.Today, listId: 1);

        Assert.Equal(0, count);
        var todos = await get.HandleAsync();
        Assert.Equal(overdueDate.Date, todos.Single().DueDate!.Value.Date);
    }

    [Fact]
    public async Task RescheduleOverdue_DoesNotAffectFutureTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new RescheduleOverdueTodosHandler(db);
        var get = new GetTodosHandler(db);

        var futureDate = DateTime.Today.AddDays(5);
        var id = await add.HandleAsync("Future task", dueDate: futureDate);

        var count = await handler.HandleAsync(DateTime.Today, listId: 1);

        Assert.Equal(0, count);
        var todos = await get.HandleAsync();
        Assert.Equal(futureDate.Date, todos.Single().DueDate!.Value.Date);
    }

    [Fact]
    public async Task RescheduleOverdue_DoesNotAffectTodosWithNoDueDate()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new RescheduleOverdueTodosHandler(db);
        var get = new GetTodosHandler(db);

        await add.HandleAsync("No date task");

        var count = await handler.HandleAsync(DateTime.Today, listId: 1);

        Assert.Equal(0, count);
        var todos = await get.HandleAsync();
        Assert.Null(todos.Single().DueDate);
    }

    [Fact]
    public async Task RescheduleOverdue_IsolatedToTargetList()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new RescheduleOverdueTodosHandler(db);
        var get = new GetTodosHandler(db);

        var overdueDate = DateTime.Today.AddDays(-2);
        // Add to default list (1) and a second list (2)
        await add.HandleAsync("Overdue in list 1", dueDate: overdueDate, listId: 1);
        await add.HandleAsync("Overdue in list 2", dueDate: overdueDate, listId: 2);

        // Only reschedule list 1
        var count = await handler.HandleAsync(DateTime.Today, listId: 1);

        Assert.Equal(1, count);
        var todos = await get.HandleAsync();
        // List 1 todo → rescheduled to today
        Assert.Equal(DateTime.Today.Date, todos.Single(t => t.ListId == 1).DueDate!.Value.Date);
        // List 2 todo → unchanged
        Assert.Equal(overdueDate.Date, todos.Single(t => t.ListId == 2).DueDate!.Value.Date);
    }

    [Fact]
    public async Task RescheduleOverdue_WhenNoneOverdue_ReturnsZero()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new RescheduleOverdueTodosHandler(db);

        await add.HandleAsync("Future task", dueDate: DateTime.Today.AddDays(3));

        var count = await handler.HandleAsync(DateTime.Today, listId: 1);

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task RescheduleOverdue_ToTomorrow_SetsCorrectDate()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var handler = new RescheduleOverdueTodosHandler(db);
        var get = new GetTodosHandler(db);

        var id = await add.HandleAsync("Overdue task", dueDate: DateTime.Today.AddDays(-1));
        var tomorrow = DateTime.Today.AddDays(1);

        await handler.HandleAsync(tomorrow, listId: 1);

        var todos = await get.HandleAsync();
        Assert.Equal(tomorrow.Date, todos.Single(t => t.Id == id).DueDate!.Value.Date);
    }

    [Fact]
    public async Task RescheduleOverdue_MixedTodos_OnlyReschedulesOverdue()
    {
        var db = await TestDatabase.CreateAsync();
        var add = new AddTodoHandler(db);
        var complete = new CompleteTodoHandler(db);
        var handler = new RescheduleOverdueTodosHandler(db);
        var get = new GetTodosHandler(db);

        var overdueId = await add.HandleAsync("Overdue", dueDate: DateTime.Today.AddDays(-3));
        var futureId = await add.HandleAsync("Future", dueDate: DateTime.Today.AddDays(3));
        var noDueId = await add.HandleAsync("No date");
        var doneId = await add.HandleAsync("Done overdue", dueDate: DateTime.Today.AddDays(-1));
        await complete.HandleAsync(doneId);

        var count = await handler.HandleAsync(DateTime.Today, listId: 1);

        Assert.Equal(1, count);
        var todos = await get.HandleAsync();
        Assert.Equal(DateTime.Today.Date, todos.Single(t => t.Id == overdueId).DueDate!.Value.Date);
        Assert.Equal(DateTime.Today.AddDays(3).Date, todos.Single(t => t.Id == futureId).DueDate!.Value.Date);
        Assert.Null(todos.Single(t => t.Id == noDueId).DueDate);
    }
}
