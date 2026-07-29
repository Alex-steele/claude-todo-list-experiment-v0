using TodoApp.Features.Todos.TimeEstimates;

namespace TodoApp.Features.Todos.EstimateAccuracy;

public enum EstimateAccuracyStatus
{
    NoEstimate,
    NoTimeLogged,
    UnderEstimate,
    OverEstimate
}

public record EstimateAccuracyResult(EstimateAccuracyStatus Status, int DeltaSeconds)
{
    public bool ShouldDisplay => Status is EstimateAccuracyStatus.UnderEstimate or EstimateAccuracyStatus.OverEstimate;
}

public static class EstimateAccuracyCalculator
{
    public static EstimateAccuracyResult Calculate(TimeEstimate estimate, int elapsedSeconds)
    {
        if (estimate == TimeEstimate.None)
            return new EstimateAccuracyResult(EstimateAccuracyStatus.NoEstimate, 0);

        if (elapsedSeconds <= 0)
            return new EstimateAccuracyResult(EstimateAccuracyStatus.NoTimeLogged, 0);

        var estimateSeconds = (int)estimate * 60;
        var delta = elapsedSeconds - estimateSeconds;

        return delta > 0
            ? new EstimateAccuracyResult(EstimateAccuracyStatus.OverEstimate, delta)
            : new EstimateAccuracyResult(EstimateAccuracyStatus.UnderEstimate, -delta);
    }
}
