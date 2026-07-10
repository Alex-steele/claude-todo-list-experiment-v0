using TodoApp.Features.Lists;
using TodoApp.Features.Todos;
using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.BulkOperations;
using TodoApp.Features.Todos.ColorLabel;
using TodoApp.Features.Todos.CompleteTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.Tags;
using TodoApp.Features.Todos.TimeEstimates;
using TodoApp.Features.Todos.Trash;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.BulkOperations;

public class BulkOperationsHandlerTests
{
    [Fact]
    public async Task CompleteAsync_MarksAllSelectedTodosComplete()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id1 = await addHandler.HandleAsync("Todo 1");
        var id2 = await addHandler.HandleAsync("Todo 2");
        var id3 = await addHandler.HandleAsync("Todo 3");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.CompleteAsync([id1, id2]);

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        Assert.True(todos.First(t => t.Id == id1).IsCompleted);
        Assert.True(todos.First(t => t.Id == id2).IsCompleted);
        Assert.False(todos.First(t => t.Id == id3).IsCompleted);
    }

    [Fact]
    public async Task DeleteAsync_RemovesAllSelectedTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id1 = await addHandler.HandleAsync("Todo 1");
        var id2 = await addHandler.HandleAsync("Todo 2");
        var id3 = await addHandler.HandleAsync("Todo 3");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.DeleteAsync([id1, id2]);

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        var remaining = Assert.Single(todos);
        Assert.Equal(id3, remaining.Id);
    }

    [Fact]
    public async Task DeleteAsync_SnapshotsDeletedTodosToTrash()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id1 = await addHandler.HandleAsync("Todo 1");
        var id2 = await addHandler.HandleAsync("Todo 2");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.DeleteAsync([id1, id2]);

        var getTrashedHandler = new GetTrashedTodosHandler(db);
        var trashed = await getTrashedHandler.HandleAsync();
        Assert.Equal(2, trashed.Count);
        Assert.Contains(trashed, t => t.Title == "Todo 1");
        Assert.Contains(trashed, t => t.Title == "Todo 2");
    }

    [Fact]
    public async Task CompleteAsync_WithEmptyList_DoesNothing()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Todo 1");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.CompleteAsync([]); // Should not throw

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        Assert.False(todos[0].IsCompleted);
    }

    [Fact]
    public async Task DeleteAsync_WithEmptyList_DoesNothing()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Todo 1");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.DeleteAsync([]); // Should not throw

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        Assert.Single(todos);
    }

    [Fact]
    public async Task CompleteAsync_AlreadyCompletedTodo_RemainsComplete()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var completeHandler = new CompleteTodoHandler(db);
        var id = await addHandler.HandleAsync("Todo 1");
        await completeHandler.HandleAsync(id); // Toggle to completed

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.CompleteAsync([id]); // Complete again via bulk

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        Assert.True(todos[0].IsCompleted);
    }

    [Fact]
    public async Task CompleteAsync_OnlyAffectsSpecifiedTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id1 = await addHandler.HandleAsync("Target");
        var id2 = await addHandler.HandleAsync("Bystander");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.CompleteAsync([id1]);

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        Assert.True(todos.First(t => t.Id == id1).IsCompleted);
        Assert.False(todos.First(t => t.Id == id2).IsCompleted);
    }

    [Fact]
    public async Task MoveAsync_MovesSelectedTodosToTargetList()
    {
        var db = await TestDatabase.CreateAsync();
        var createListHandler = new CreateListHandler(db);
        var addHandler = new AddTodoHandler(db);

        var workListId = await createListHandler.HandleAsync("Work");
        var id1 = await addHandler.HandleAsync("Task 1", listId: 1);
        var id2 = await addHandler.HandleAsync("Task 2", listId: 1);
        var id3 = await addHandler.HandleAsync("Task 3", listId: 1);

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.MoveAsync([id1, id2], workListId);

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        Assert.Equal(workListId, todos.First(t => t.Id == id1).ListId);
        Assert.Equal(workListId, todos.First(t => t.Id == id2).ListId);
        Assert.Equal(1, todos.First(t => t.Id == id3).ListId);
    }

    [Fact]
    public async Task MoveAsync_WithEmptyList_DoesNothing()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Task 1", listId: 1);

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.MoveAsync([], 99); // Should not throw

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        Assert.Equal(1, todos[0].ListId);
    }

    [Fact]
    public async Task MoveAsync_OnlyAffectsSpecifiedTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var createListHandler = new CreateListHandler(db);
        var addHandler = new AddTodoHandler(db);

        var workListId = await createListHandler.HandleAsync("Work");
        var id1 = await addHandler.HandleAsync("Target", listId: 1);
        var id2 = await addHandler.HandleAsync("Bystander", listId: 1);

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.MoveAsync([id1], workListId);

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        Assert.Equal(workListId, todos.First(t => t.Id == id1).ListId);
        Assert.Equal(1, todos.First(t => t.Id == id2).ListId);
    }

    [Fact]
    public async Task AddTagAsync_AppliesTagToAllSelectedTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id1 = await addHandler.HandleAsync("Todo 1");
        var id2 = await addHandler.HandleAsync("Todo 2");
        var id3 = await addHandler.HandleAsync("Todo 3");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.AddTagAsync([id1, id2], "work");

        var getTagsHandler = new GetTodoTagsHandler(db);
        var tags = await getTagsHandler.HandleAsync([id1, id2, id3]);
        Assert.Contains(tags[id1], t => t.Name == "work");
        Assert.Contains(tags[id2], t => t.Name == "work");
        Assert.DoesNotContain(tags.GetValueOrDefault(id3, []), t => t.Name == "work");
    }

    [Fact]
    public async Task AddTagAsync_NormalizesTagToLowercase()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id = await addHandler.HandleAsync("Todo");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.AddTagAsync([id], "  Work  ");

        var getTagsHandler = new GetTodoTagsHandler(db);
        var tags = await getTagsHandler.HandleAsync([id]);
        Assert.Contains(tags[id], t => t.Name == "work");
    }

    [Fact]
    public async Task AddTagAsync_DoesNotDuplicateExistingTag()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var addTagHandler = new AddTagHandler(db);
        var id = await addHandler.HandleAsync("Todo");
        await addTagHandler.HandleAsync(id, "work");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.AddTagAsync([id], "work");

        var getTagsHandler = new GetTodoTagsHandler(db);
        var tags = await getTagsHandler.HandleAsync([id]);
        Assert.Single(tags[id], t => t.Name == "work");
    }

    [Fact]
    public async Task AddTagAsync_WithEmptyList_DoesNothing()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id = await addHandler.HandleAsync("Todo");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.AddTagAsync([], "work");

        var getTagsHandler = new GetTodoTagsHandler(db);
        var tags = await getTagsHandler.HandleAsync([id]);
        Assert.Empty(tags.GetValueOrDefault(id, []));
    }

    [Fact]
    public async Task AddTagAsync_WithEmptyTagName_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id = await addHandler.HandleAsync("Todo");

        var bulkHandler = new BulkOperationsHandler(db);
        await Assert.ThrowsAsync<ArgumentException>(() => bulkHandler.AddTagAsync([id], "   "));
    }

    [Fact]
    public async Task SetPriorityAsync_SetsAllSelectedTodosToSpecifiedPriority()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id1 = await addHandler.HandleAsync("Todo 1");
        var id2 = await addHandler.HandleAsync("Todo 2");
        var id3 = await addHandler.HandleAsync("Todo 3");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.SetPriorityAsync([id1, id2], TodoPriority.High);

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        Assert.Equal(TodoPriority.High, todos.First(t => t.Id == id1).Priority);
        Assert.Equal(TodoPriority.High, todos.First(t => t.Id == id2).Priority);
        Assert.Equal(TodoPriority.None, todos.First(t => t.Id == id3).Priority);
    }

    [Fact]
    public async Task SetPriorityAsync_WithEmptyList_DoesNothing()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        await addHandler.HandleAsync("Todo 1", priority: TodoPriority.High);

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.SetPriorityAsync([], TodoPriority.Low);

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        Assert.Equal(TodoPriority.High, todos[0].Priority);
    }

    [Fact]
    public async Task SetPriorityAsync_CanClearPriorityToNone()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id = await addHandler.HandleAsync("Todo", priority: TodoPriority.High);

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.SetPriorityAsync([id], TodoPriority.None);

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        Assert.Equal(TodoPriority.None, todos.First(t => t.Id == id).Priority);
    }

    [Fact]
    public async Task SetPriorityAsync_OnlyAffectsSpecifiedTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id1 = await addHandler.HandleAsync("Target");
        var id2 = await addHandler.HandleAsync("Bystander");

        var bulkHandler = new BulkOperationsHandler(db);
        await bulkHandler.SetPriorityAsync([id1], TodoPriority.Medium);

        var getHandler = new GetTodosHandler(db);
        var todos = await getHandler.HandleAsync();
        Assert.Equal(TodoPriority.Medium, todos.First(t => t.Id == id1).Priority);
        Assert.Equal(TodoPriority.None, todos.First(t => t.Id == id2).Priority);
    }

    // ── SetDueDateAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task SetDueDateAsync_SetsDueDateOnSelectedTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var id1 = await addHandler.HandleAsync("Task 1");
        var id2 = await addHandler.HandleAsync("Task 2");
        var id3 = await addHandler.HandleAsync("Task 3");

        var dueDate = new DateTime(2026, 12, 31);
        await new BulkOperationsHandler(db).SetDueDateAsync([id1, id2], dueDate);

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Equal(dueDate.Date, todos.First(t => t.Id == id1).DueDate!.Value.Date);
        Assert.Equal(dueDate.Date, todos.First(t => t.Id == id2).DueDate!.Value.Date);
        Assert.Null(todos.First(t => t.Id == id3).DueDate);
    }

    [Fact]
    public async Task SetDueDateAsync_ClearsDueDateWhenPassedNull()
    {
        var db = await TestDatabase.CreateAsync();
        var dueDate = new DateTime(2026, 7, 1);
        var id1 = await new AddTodoHandler(db).HandleAsync("Task 1", dueDate: dueDate);
        var id2 = await new AddTodoHandler(db).HandleAsync("Task 2", dueDate: dueDate);

        await new BulkOperationsHandler(db).SetDueDateAsync([id1], null);

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Null(todos.First(t => t.Id == id1).DueDate);
        Assert.NotNull(todos.First(t => t.Id == id2).DueDate);
    }

    [Fact]
    public async Task SetDueDateAsync_EmptyList_DoesNothing()
    {
        var db = await TestDatabase.CreateAsync();
        var id = await new AddTodoHandler(db).HandleAsync("Task");

        await new BulkOperationsHandler(db).SetDueDateAsync([], new DateTime(2026, 12, 31));

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Null(todos.First(t => t.Id == id).DueDate);
    }

    [Fact]
    public async Task SetDueDateAsync_OverwritesExistingDueDate()
    {
        var db = await TestDatabase.CreateAsync();
        var original = new DateTime(2026, 1, 15);
        var updated  = new DateTime(2026, 6, 30);
        var id = await new AddTodoHandler(db).HandleAsync("Task", dueDate: original);

        await new BulkOperationsHandler(db).SetDueDateAsync([id], updated);

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Equal(updated.Date, todos.First(t => t.Id == id).DueDate!.Value.Date);
    }

    // ── SetTimeEstimateAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task SetTimeEstimateAsync_SetsEstimateOnSelectedTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var id1 = await new AddTodoHandler(db).HandleAsync("Task 1");
        var id2 = await new AddTodoHandler(db).HandleAsync("Task 2");
        var id3 = await new AddTodoHandler(db).HandleAsync("Task 3");

        await new BulkOperationsHandler(db).SetTimeEstimateAsync([id1, id2], TimeEstimate.OneHour);

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Equal(TimeEstimate.OneHour, todos.First(t => t.Id == id1).TimeEstimate);
        Assert.Equal(TimeEstimate.OneHour, todos.First(t => t.Id == id2).TimeEstimate);
        Assert.Equal(TimeEstimate.None, todos.First(t => t.Id == id3).TimeEstimate);
    }

    [Fact]
    public async Task SetTimeEstimateAsync_WithEmptyList_DoesNothing()
    {
        var db = await TestDatabase.CreateAsync();
        var id = await new AddTodoHandler(db).HandleAsync("Task", timeEstimate: TimeEstimate.TwoHours);

        await new BulkOperationsHandler(db).SetTimeEstimateAsync([], TimeEstimate.FifteenMinutes);

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Equal(TimeEstimate.TwoHours, todos.First(t => t.Id == id).TimeEstimate);
    }

    [Fact]
    public async Task SetTimeEstimateAsync_CanClearEstimateToNone()
    {
        var db = await TestDatabase.CreateAsync();
        var id = await new AddTodoHandler(db).HandleAsync("Task", timeEstimate: TimeEstimate.FourHours);

        await new BulkOperationsHandler(db).SetTimeEstimateAsync([id], TimeEstimate.None);

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Equal(TimeEstimate.None, todos.First(t => t.Id == id).TimeEstimate);
    }

    [Fact]
    public async Task SetTimeEstimateAsync_OnlyAffectsSpecifiedTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var id1 = await new AddTodoHandler(db).HandleAsync("Target");
        var id2 = await new AddTodoHandler(db).HandleAsync("Bystander");

        await new BulkOperationsHandler(db).SetTimeEstimateAsync([id1], TimeEstimate.ThirtyMinutes);

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Equal(TimeEstimate.ThirtyMinutes, todos.First(t => t.Id == id1).TimeEstimate);
        Assert.Equal(TimeEstimate.None, todos.First(t => t.Id == id2).TimeEstimate);
    }

    [Fact]
    public async Task SetTimeEstimateAsync_OverwritesExistingEstimate()
    {
        var db = await TestDatabase.CreateAsync();
        var id = await new AddTodoHandler(db).HandleAsync("Task", timeEstimate: TimeEstimate.OneHour);

        await new BulkOperationsHandler(db).SetTimeEstimateAsync([id], TimeEstimate.OneDay);

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Equal(TimeEstimate.OneDay, todos.First(t => t.Id == id).TimeEstimate);
    }

    // ── SetColorLabelAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task SetColorLabelAsync_SetsColorOnSelectedTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var id1 = await new AddTodoHandler(db).HandleAsync("Task 1");
        var id2 = await new AddTodoHandler(db).HandleAsync("Task 2");
        var id3 = await new AddTodoHandler(db).HandleAsync("Task 3");

        await new BulkOperationsHandler(db).SetColorLabelAsync([id1, id2], TodoColorLabel.Red);

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Equal(TodoColorLabel.Red, todos.First(t => t.Id == id1).ColorLabel);
        Assert.Equal(TodoColorLabel.Red, todos.First(t => t.Id == id2).ColorLabel);
        Assert.Equal(TodoColorLabel.None, todos.First(t => t.Id == id3).ColorLabel);
    }

    [Fact]
    public async Task SetColorLabelAsync_WithEmptyList_DoesNothing()
    {
        var db = await TestDatabase.CreateAsync();
        var id = await new AddTodoHandler(db).HandleAsync("Task");

        await new BulkOperationsHandler(db).SetColorLabelAsync([], TodoColorLabel.Blue);

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Equal(TodoColorLabel.None, todos.First(t => t.Id == id).ColorLabel);
    }

    [Fact]
    public async Task SetColorLabelAsync_CanClearColorToNone()
    {
        var db = await TestDatabase.CreateAsync();
        var setColorHandler = new SetColorLabelHandler(db);
        var id = await new AddTodoHandler(db).HandleAsync("Task");
        await setColorHandler.HandleAsync(id, TodoColorLabel.Green);

        await new BulkOperationsHandler(db).SetColorLabelAsync([id], TodoColorLabel.None);

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Equal(TodoColorLabel.None, todos.First(t => t.Id == id).ColorLabel);
    }

    [Fact]
    public async Task SetColorLabelAsync_OnlyAffectsSpecifiedTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var id1 = await new AddTodoHandler(db).HandleAsync("Target");
        var id2 = await new AddTodoHandler(db).HandleAsync("Bystander");

        await new BulkOperationsHandler(db).SetColorLabelAsync([id1], TodoColorLabel.Purple);

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Equal(TodoColorLabel.Purple, todos.First(t => t.Id == id1).ColorLabel);
        Assert.Equal(TodoColorLabel.None, todos.First(t => t.Id == id2).ColorLabel);
    }

    [Fact]
    public async Task SetColorLabelAsync_OverwritesExistingColor()
    {
        var db = await TestDatabase.CreateAsync();
        var setColorHandler = new SetColorLabelHandler(db);
        var id = await new AddTodoHandler(db).HandleAsync("Task");
        await setColorHandler.HandleAsync(id, TodoColorLabel.Orange);

        await new BulkOperationsHandler(db).SetColorLabelAsync([id], TodoColorLabel.Blue);

        var todos = await new GetTodosHandler(db).HandleAsync();
        Assert.Equal(TodoColorLabel.Blue, todos.First(t => t.Id == id).ColorLabel);
    }
}
