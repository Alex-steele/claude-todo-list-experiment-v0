using TodoApp.Features.Todos;
using TodoApp.Features.Todos.ColorLabel;
using TodoApp.Features.Todos.FilterSortTodos;

namespace TodoApp.Features.FilterPresets;

public record FilterPresetOptions(
    TodoStatusFilter StatusFilter,
    TodoPriority? PriorityFilter,
    TodoDateFilter DateFilter,
    string? TagFilter,
    TodoColorLabel? ColorFilter,
    TodoTimeEstimateFilter TimeEstimateFilter,
    TodoSortOrder SortOrder
);

public record FilterPreset(int Id, string Name, FilterPresetOptions Options);
