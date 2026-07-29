using TodoApp.Features.Todos.EstimateAccuracy;
using TodoApp.Features.Todos.TimeEstimates;
using Xunit;

namespace TodoApp.Tests.Features.Todos.EstimateAccuracy;

public class EstimateAccuracyCalculatorTests
{
    [Fact]
    public void Calculate_NoEstimate_ReturnsNoEstimate()
    {
        var result = EstimateAccuracyCalculator.Calculate(TimeEstimate.None, 600);

        Assert.Equal(EstimateAccuracyStatus.NoEstimate, result.Status);
        Assert.False(result.ShouldDisplay);
    }

    [Fact]
    public void Calculate_NoTimeLogged_ReturnsNoTimeLogged()
    {
        var result = EstimateAccuracyCalculator.Calculate(TimeEstimate.OneHour, 0);

        Assert.Equal(EstimateAccuracyStatus.NoTimeLogged, result.Status);
        Assert.False(result.ShouldDisplay);
    }

    [Fact]
    public void Calculate_ElapsedLessThanEstimate_ReturnsUnderEstimateWithRemainingDelta()
    {
        // 15 min estimate, 10 min elapsed -> 5 min under
        var result = EstimateAccuracyCalculator.Calculate(TimeEstimate.FifteenMinutes, 10 * 60);

        Assert.Equal(EstimateAccuracyStatus.UnderEstimate, result.Status);
        Assert.Equal(5 * 60, result.DeltaSeconds);
        Assert.True(result.ShouldDisplay);
    }

    [Fact]
    public void Calculate_ElapsedExactlyEqualsEstimate_ReturnsUnderEstimateWithZeroDelta()
    {
        var result = EstimateAccuracyCalculator.Calculate(TimeEstimate.ThirtyMinutes, 30 * 60);

        Assert.Equal(EstimateAccuracyStatus.UnderEstimate, result.Status);
        Assert.Equal(0, result.DeltaSeconds);
        Assert.True(result.ShouldDisplay);
    }

    [Fact]
    public void Calculate_ElapsedGreaterThanEstimate_ReturnsOverEstimateWithExcessDelta()
    {
        // 30 min estimate, 50 min elapsed -> 20 min over
        var result = EstimateAccuracyCalculator.Calculate(TimeEstimate.ThirtyMinutes, 50 * 60);

        Assert.Equal(EstimateAccuracyStatus.OverEstimate, result.Status);
        Assert.Equal(20 * 60, result.DeltaSeconds);
        Assert.True(result.ShouldDisplay);
    }
}
