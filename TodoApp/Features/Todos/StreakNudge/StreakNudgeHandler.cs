namespace TodoApp.Features.Todos.StreakNudge;

public class StreakNudgeHandler
{
    public bool ShouldNudge(int currentStreak, int completedToday)
        => currentStreak > 0 && completedToday == 0;
}
