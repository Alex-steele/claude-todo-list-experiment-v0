using TodoApp.Features.Todos.GetTodos;

namespace TodoApp.Features.Todos.TimeTracking;

public static class TimeTrackingCalculator
{
    public static int GetElapsedSeconds(TodoSummary todo, DateTime now)
    {
        var runningSeconds = todo.TimerStartedAt is { } startedAt
            ? Math.Max(0, (int)(now - startedAt).TotalSeconds)
            : 0;

        return todo.TimeSpentSeconds + runningSeconds;
    }

    public static string FormatDuration(int totalSeconds)
    {
        if (totalSeconds < 60)
            return $"{totalSeconds}s";

        var totalMinutes = totalSeconds / 60;
        if (totalMinutes < 60)
            return $"{totalMinutes}m";

        var hours = totalMinutes / 60;
        var minutes = totalMinutes % 60;
        return minutes == 0 ? $"{hours}h" : $"{hours}h {minutes}m";
    }
}
