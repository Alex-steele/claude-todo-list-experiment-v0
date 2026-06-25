using TodoApp.Features.Todos;
using TodoApp.Features.Todos.RecurringTodos;
using TodoApp.Features.Todos.Templates;
using TodoApp.Features.Todos.TimeEstimates;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Templates;

public class TemplateHandlerTests
{
    [Fact]
    public async Task GetTemplates_ReturnsEmpty_WhenNoTemplatesExist()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new GetTemplatesHandler(db);

        var result = await handler.HandleAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task SaveTemplate_PersistsTemplate_AndGetReturnsIt()
    {
        var db = await TestDatabase.CreateAsync();
        var save = new SaveTemplateHandler(db);
        var get = new GetTemplatesHandler(db);

        await save.HandleAsync("Work Sprint", TodoPriority.High, TimeEstimate.TwoHours, RecurrenceRule.None);

        var templates = await get.HandleAsync();
        Assert.Single(templates);
        Assert.Equal("Work Sprint", templates[0].Name);
        Assert.Equal(TodoPriority.High, templates[0].Priority);
        Assert.Equal(TimeEstimate.TwoHours, templates[0].TimeEstimate);
        Assert.Equal(RecurrenceRule.None, templates[0].Recurrence);
    }

    [Fact]
    public async Task SaveTemplate_WithRecurrence_PersistsRecurrence()
    {
        var db = await TestDatabase.CreateAsync();
        var save = new SaveTemplateHandler(db);
        var get = new GetTemplatesHandler(db);

        await save.HandleAsync("Daily Standup", TodoPriority.None, TimeEstimate.FifteenMinutes, RecurrenceRule.Daily);

        var templates = await get.HandleAsync();
        Assert.Single(templates);
        Assert.Equal(RecurrenceRule.Daily, templates[0].Recurrence);
        Assert.Equal(TimeEstimate.FifteenMinutes, templates[0].TimeEstimate);
    }

    [Fact]
    public async Task SaveTemplate_TrimsTemplateName()
    {
        var db = await TestDatabase.CreateAsync();
        var save = new SaveTemplateHandler(db);
        var get = new GetTemplatesHandler(db);

        await save.HandleAsync("  My Template  ", TodoPriority.Low, TimeEstimate.None, RecurrenceRule.None);

        var templates = await get.HandleAsync();
        Assert.Equal("My Template", templates[0].Name);
    }

    [Fact]
    public async Task SaveTemplate_EmptyName_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        var save = new SaveTemplateHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            save.HandleAsync("", TodoPriority.None, TimeEstimate.None, RecurrenceRule.None));
    }

    [Fact]
    public async Task SaveTemplate_WhitespaceName_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        var save = new SaveTemplateHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            save.HandleAsync("   ", TodoPriority.None, TimeEstimate.None, RecurrenceRule.None));
    }

    [Fact]
    public async Task DeleteTemplate_RemovesTemplate()
    {
        var db = await TestDatabase.CreateAsync();
        var save = new SaveTemplateHandler(db);
        var get = new GetTemplatesHandler(db);
        var delete = new DeleteTemplateHandler(db);

        var id = await save.HandleAsync("To Delete", TodoPriority.None, TimeEstimate.None, RecurrenceRule.None);
        await delete.HandleAsync(id);

        var templates = await get.HandleAsync();
        Assert.Empty(templates);
    }

    [Fact]
    public async Task DeleteTemplate_DoesNotAffectOtherTemplates()
    {
        var db = await TestDatabase.CreateAsync();
        var save = new SaveTemplateHandler(db);
        var get = new GetTemplatesHandler(db);
        var delete = new DeleteTemplateHandler(db);

        var id1 = await save.HandleAsync("Keep", TodoPriority.Low, TimeEstimate.None, RecurrenceRule.None);
        var id2 = await save.HandleAsync("Delete", TodoPriority.High, TimeEstimate.None, RecurrenceRule.None);

        await delete.HandleAsync(id2);

        var templates = await get.HandleAsync();
        Assert.Single(templates);
        Assert.Equal(id1, templates[0].Id);
        Assert.Equal("Keep", templates[0].Name);
    }

    [Fact]
    public async Task GetTemplates_ReturnsAllTemplates_InInsertionOrder()
    {
        var db = await TestDatabase.CreateAsync();
        var save = new SaveTemplateHandler(db);
        var get = new GetTemplatesHandler(db);

        await save.HandleAsync("First", TodoPriority.High, TimeEstimate.None, RecurrenceRule.None);
        await save.HandleAsync("Second", TodoPriority.Medium, TimeEstimate.OneHour, RecurrenceRule.None);
        await save.HandleAsync("Third", TodoPriority.Low, TimeEstimate.None, RecurrenceRule.Weekly);

        var templates = await get.HandleAsync();
        Assert.Equal(3, templates.Count);
        Assert.Equal("First", templates[0].Name);
        Assert.Equal("Second", templates[1].Name);
        Assert.Equal("Third", templates[2].Name);
    }

    [Fact]
    public async Task SaveTemplate_ReturnsNewId()
    {
        var db = await TestDatabase.CreateAsync();
        var save = new SaveTemplateHandler(db);

        var id1 = await save.HandleAsync("A", TodoPriority.None, TimeEstimate.None, RecurrenceRule.None);
        var id2 = await save.HandleAsync("B", TodoPriority.None, TimeEstimate.None, RecurrenceRule.None);

        Assert.True(id2 > id1);
    }
}
