namespace TodoApp.Features.Todos.PomodoroTimer;

public static class PomodoroCalculator
{
    public const int WorkDurationSeconds = 25 * 60;
    public const int BreakDurationSeconds = 5 * 60;

    public static int GetPhaseDurationSeconds(PomodoroPhase phase) =>
        phase == PomodoroPhase.Work ? WorkDurationSeconds : BreakDurationSeconds;

    public static int GetRemainingSeconds(PomodoroPhase phase, DateTime phaseStartedAt, DateTime now)
    {
        var elapsed = Math.Max(0, (int)(now - phaseStartedAt).TotalSeconds);
        return Math.Max(0, GetPhaseDurationSeconds(phase) - elapsed);
    }

    public static bool IsPhaseComplete(PomodoroPhase phase, DateTime phaseStartedAt, DateTime now) =>
        GetRemainingSeconds(phase, phaseStartedAt, now) == 0;

    public static string FormatCountdown(int totalSeconds)
    {
        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;
        return $"{minutes}:{seconds:D2}";
    }
}
