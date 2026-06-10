using TodoApp.Features.Todos.AddTodo;
using TodoApp.Features.Todos.EditTodo;
using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.GetTodosStats;
using TodoApp.Features.Todos.TimeEstimates;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.TimeEstimates;

public class TimeEstimateTests
{
    [Fact]
    public async Task AddTodo_WithTimeEstimate_PersistsEstimate()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id = await addHandler.HandleAsync("Estimated task", timeEstimate: TimeEstimate.OneHour);

        var todos = await getHandler.HandleAsync();
        Assert.Equal(TimeEstimate.OneHour, todos.Single(t => t.Id == id).TimeEstimate);
    }

    [Fact]
    public async Task AddTodo_WithNoEstimate_DefaultsToNone()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id = await addHandler.HandleAsync("No estimate task");

        var todos = await getHandler.HandleAsync();
        Assert.Equal(TimeEstimate.None, todos.Single(t => t.Id == id).TimeEstimate);
    }

    [Fact]
    public async Task EditTodo_UpdatesTimeEstimate()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var editHandler = new EditTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id = await addHandler.HandleAsync("Task");
        await editHandler.HandleAsync(id, "Task", timeEstimate: TimeEstimate.TwoHours);

        var todos = await getHandler.HandleAsync();
        Assert.Equal(TimeEstimate.TwoHours, todos.Single(t => t.Id == id).TimeEstimate);
    }

    [Fact]
    public async Task EditTodo_ClearsTimeEstimate()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var editHandler = new EditTodoHandler(db);
        var getHandler = new GetTodosHandler(db);

        var id = await addHandler.HandleAsync("Task", timeEstimate: TimeEstimate.FourHours);
        await editHandler.HandleAsync(id, "Task", timeEstimate: TimeEstimate.None);

        var todos = await getHandler.HandleAsync();
        Assert.Equal(TimeEstimate.None, todos.Single(t => t.Id == id).TimeEstimate);
    }

    [Fact]
    public async Task Stats_TotalEstimatedMinutes_SumsActiveEstimates()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getHandler = new GetTodosHandler(db);
        var statsHandler = new GetTodosStatsHandler();

        // 30 min + 60 min = 90 min total (both active)
        await addHandler.HandleAsync("Task A", timeEstimate: TimeEstimate.ThirtyMinutes);
        await addHandler.HandleAsync("Task B", timeEstimate: TimeEstimate.OneHour);

        var todos = await getHandler.HandleAsync();
        var stats = statsHandler.Handle(todos);

        Assert.Equal(90, stats.TotalEstimatedMinutes);
    }

    [Fact]
    public async Task Stats_TotalEstimatedMinutes_ExcludesCompletedTodos()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var completeHandler = new TodoApp.Features.Todos.CompleteTodo.CompleteTodoHandler(db);
        var getHandler = new GetTodosHandler(db);
        var statsHandler = new GetTodosStatsHandler();

        var activeId = await addHandler.HandleAsync("Active", timeEstimate: TimeEstimate.OneHour);
        var completedId = await addHandler.HandleAsync("Done", timeEstimate: TimeEstimate.TwoHours);
        await completeHandler.HandleAsync(completedId);

        var todos = await getHandler.HandleAsync();
        var stats = statsHandler.Handle(todos);

        // Only the active 60-min task should count
        Assert.Equal(60, stats.TotalEstimatedMinutes);
    }

    [Fact]
    public async Task Stats_TotalEstimatedMinutes_ZeroWhenNoEstimates()
    {
        var db = await TestDatabase.CreateAsync();
        var addHandler = new AddTodoHandler(db);
        var getHandler = new GetTodosHandler(db);
        var statsHandler = new GetTodosStatsHandler();

        await addHandler.HandleAsync("No estimate");

        var todos = await getHandler.HandleAsync();
        var stats = statsHandler.Handle(todos);

        Assert.Equal(0, stats.TotalEstimatedMinutes);
    }

    [Fact]
    public void FormatEstimatedTime_UnderOneHour_ShowsMinutes()
    {
        // 30 minutes = "30 min"
        var result = FormatEstimatedTime(30);
        Assert.Equal("30 min", result);
    }

    [Fact]
    public void FormatEstimatedTime_ExactHours_ShowsHoursOnly()
    {
        // 120 minutes = "2 h"
        var result = FormatEstimatedTime(120);
        Assert.Equal("2 h", result);
    }

    [Fact]
    public void FormatEstimatedTime_HoursAndMinutes_ShowsBoth()
    {
        // 90 minutes = "1 h 30 min"
        var result = FormatEstimatedTime(90);
        Assert.Equal("1 h 30 min", result);
    }

    private static string FormatEstimatedTime(int minutes)
    {
        if (minutes < 60)
            return $"{minutes} min";
        var hours = minutes / 60;
        var remainder = minutes % 60;
        return remainder == 0 ? $"{hours} h" : $"{hours} h {remainder} min";
    }
}
