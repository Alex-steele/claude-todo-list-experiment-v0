namespace TodoApp.Features.Todos.FilterSortTodos;

public enum TodoStatusFilter
{
    All = 0,
    Active = 1,
    Completed = 2
}

public enum TodoSortOrder
{
    Newest = 0,
    Oldest = 1,
    DueDateAsc = 2,
    PriorityDesc = 3,
    Manual = 4
}
