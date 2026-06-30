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
    Manual = 4,
    TitleAsc = 5,
    TitleDesc = 6,
    TimeEstimateAsc = 7,
    TimeEstimateDesc = 8
}

public enum TodoDateFilter
{
    None = 0,
    Overdue = 1,
    DueToday = 2,
    DueThisWeek = 3
}

public enum TodoTimeEstimateFilter
{
    Any = 0,
    NoEstimate = 1,
    Max15Min = 15,
    Max30Min = 30,
    Max1Hour = 60,
    Max2Hours = 120
}

public enum TodoStalenessFilter
{
    Any = 0,
    OneWeekPlus = 7,
    TwoWeeksPlus = 14,
    OneMonthPlus = 30
}
