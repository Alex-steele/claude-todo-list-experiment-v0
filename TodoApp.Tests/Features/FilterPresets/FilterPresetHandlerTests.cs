using TodoApp.Features.FilterPresets;
using TodoApp.Features.Todos;
using TodoApp.Features.Todos.ColorLabel;
using TodoApp.Features.Todos.FilterSortTodos;
using TodoApp.Tests.Infrastructure;
using Xunit;

namespace TodoApp.Tests.Features.FilterPresets;

public class FilterPresetHandlerTests
{
    private static FilterPresetOptions DefaultOptions() => new(
        TodoStatusFilter.Active,
        TodoPriority.High,
        TodoDateFilter.DueToday,
        "work",
        TodoColorLabel.Blue,
        TodoTimeEstimateFilter.Max1Hour,
        TodoSortOrder.PriorityDesc);

    [Fact]
    public async Task SavePreset_ValidName_IsPersisted()
    {
        var db = await TestDatabase.CreateAsync();
        var saveHandler = new SaveFilterPresetHandler(db);
        var getHandler = new GetFilterPresetsHandler(db);

        var id = await saveHandler.HandleAsync("Morning tasks", DefaultOptions());

        Assert.True(id > 0);
        var presets = await getHandler.HandleAsync();
        Assert.Single(presets);
        Assert.Equal("Morning tasks", presets[0].Name);
    }

    [Fact]
    public async Task SavePreset_EmptyName_ThrowsArgumentException()
    {
        var db = await TestDatabase.CreateAsync();
        var handler = new SaveFilterPresetHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync("", DefaultOptions()));
        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync("   ", DefaultOptions()));
    }

    [Fact]
    public async Task SavePreset_PreservesAllFilterOptions()
    {
        var db = await TestDatabase.CreateAsync();
        var saveHandler = new SaveFilterPresetHandler(db);
        var getHandler = new GetFilterPresetsHandler(db);

        var options = DefaultOptions();
        await saveHandler.HandleAsync("Test preset", options);

        var presets = await getHandler.HandleAsync();
        var o = presets[0].Options;
        Assert.Equal(options.StatusFilter, o.StatusFilter);
        Assert.Equal(options.PriorityFilter, o.PriorityFilter);
        Assert.Equal(options.DateFilter, o.DateFilter);
        Assert.Equal(options.TagFilter, o.TagFilter);
        Assert.Equal(options.ColorFilter, o.ColorFilter);
        Assert.Equal(options.TimeEstimateFilter, o.TimeEstimateFilter);
        Assert.Equal(options.SortOrder, o.SortOrder);
    }

    [Fact]
    public async Task GetPresets_ReturnsMultiplePresetsInInsertOrder()
    {
        var db = await TestDatabase.CreateAsync();
        var saveHandler = new SaveFilterPresetHandler(db);
        var getHandler = new GetFilterPresetsHandler(db);

        await saveHandler.HandleAsync("First", DefaultOptions());
        await saveHandler.HandleAsync("Second", DefaultOptions());
        await saveHandler.HandleAsync("Third", DefaultOptions());

        var presets = await getHandler.HandleAsync();
        Assert.Equal(3, presets.Count);
        Assert.Equal(["First", "Second", "Third"], presets.Select(p => p.Name).ToList());
    }

    [Fact]
    public async Task GetPresets_ReturnsEmptyList_WhenNoneSaved()
    {
        var db = await TestDatabase.CreateAsync();
        var getHandler = new GetFilterPresetsHandler(db);

        var presets = await getHandler.HandleAsync();
        Assert.Empty(presets);
    }

    [Fact]
    public async Task DeletePreset_RemovesPreset()
    {
        var db = await TestDatabase.CreateAsync();
        var saveHandler = new SaveFilterPresetHandler(db);
        var getHandler = new GetFilterPresetsHandler(db);
        var deleteHandler = new DeleteFilterPresetHandler(db);

        var id = await saveHandler.HandleAsync("To delete", DefaultOptions());
        await deleteHandler.HandleAsync(id);

        var presets = await getHandler.HandleAsync();
        Assert.Empty(presets);
    }

    [Fact]
    public async Task DeletePreset_OnlyRemovesSpecifiedPreset()
    {
        var db = await TestDatabase.CreateAsync();
        var saveHandler = new SaveFilterPresetHandler(db);
        var getHandler = new GetFilterPresetsHandler(db);
        var deleteHandler = new DeleteFilterPresetHandler(db);

        var id1 = await saveHandler.HandleAsync("Keep", DefaultOptions());
        var id2 = await saveHandler.HandleAsync("Delete", DefaultOptions());

        await deleteHandler.HandleAsync(id2);

        var presets = await getHandler.HandleAsync();
        Assert.Single(presets);
        Assert.Equal("Keep", presets[0].Name);
    }

    [Fact]
    public async Task SavePreset_WithNullableNoneOptions_Roundtrips()
    {
        var db = await TestDatabase.CreateAsync();
        var saveHandler = new SaveFilterPresetHandler(db);
        var getHandler = new GetFilterPresetsHandler(db);

        var options = new FilterPresetOptions(
            TodoStatusFilter.All,
            null,
            TodoDateFilter.None,
            null,
            null,
            TodoTimeEstimateFilter.Any,
            TodoSortOrder.Newest);

        await saveHandler.HandleAsync("All default", options);

        var presets = await getHandler.HandleAsync();
        var o = presets[0].Options;
        Assert.Null(o.PriorityFilter);
        Assert.Null(o.TagFilter);
        Assert.Null(o.ColorFilter);
        Assert.Equal(TodoStatusFilter.All, o.StatusFilter);
    }
}
