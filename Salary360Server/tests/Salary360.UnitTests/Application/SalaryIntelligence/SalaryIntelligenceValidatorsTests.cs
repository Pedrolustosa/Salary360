using Salary360.Application.SalaryIntelligence.Enums;
using Salary360.Application.SalaryIntelligence.Queries;

namespace Salary360.UnitTests.Application.SalaryIntelligence;

public sealed class SalaryIntelligenceValidatorsTests
{
    private readonly FixedTimeProvider time = new(new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero));

    [Fact]
    public void Summary_ValidPeriod_Passes()
    {
        var result = new GetSalaryIntelligenceSummaryQueryValidator(time)
            .Validate(new GetSalaryIntelligenceSummaryQuery("2024-01", "2024-06"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Summary_InvertedPeriod_FailsInvalidPeriod()
    {
        var result = new GetSalaryIntelligenceSummaryQueryValidator(time)
            .Validate(new GetSalaryIntelligenceSummaryQuery("2024-06", "2024-01"));

        Assert.Contains(result.Errors, error => error.ErrorCode == "invalid_period");
    }

    [Fact]
    public void Summary_FutureCompetence_Fails()
    {
        var result = new GetSalaryIntelligenceSummaryQueryValidator(time)
            .Validate(new GetSalaryIntelligenceSummaryQuery("2024-01", "2024-07"));

        Assert.Contains(result.Errors, error => error.ErrorCode == "future_competence");
    }

    [Fact]
    public void Summary_PeriodLongerThan120Months_Fails()
    {
        var result = new GetSalaryIntelligenceSummaryQueryValidator(time)
            .Validate(new GetSalaryIntelligenceSummaryQuery("2014-01", "2024-06"));

        Assert.Contains(result.Errors, error => error.ErrorCode == "period_too_long");
    }

    [Fact]
    public void Summary_OnlyFromProvided_FailsInvalidPeriod()
    {
        var result = new GetSalaryIntelligenceSummaryQueryValidator(time)
            .Validate(new GetSalaryIntelligenceSummaryQuery("2024-01", null));

        Assert.Contains(result.Errors, error => error.ErrorCode == "invalid_period");
    }

    [Fact]
    public void Summary_InvalidBasis_Fails()
    {
        var result = new GetSalaryIntelligenceSummaryQueryValidator(time)
            .Validate(new GetSalaryIntelligenceSummaryQuery(null, null, (SalaryBasis)99));

        Assert.Contains(result.Errors, error => error.ErrorCode == "invalid_basis");
    }

    [Fact]
    public void Summary_MalformedCompetence_FailsInvalidPeriod()
    {
        var result = new GetSalaryIntelligenceSummaryQueryValidator(time)
            .Validate(new GetSalaryIntelligenceSummaryQuery("2024/01", "2024-06"));

        Assert.Contains(result.Errors, error => error.ErrorCode == "invalid_period");
    }

    [Fact]
    public void Projection_InvalidHorizon_Fails()
    {
        var result = new GetSalaryProjectionQueryValidator(time)
            .Validate(new GetSalaryProjectionQuery("2024-01", "2024-06", SalaryBasis.Net, 5));

        Assert.Contains(result.Errors, error => error.ErrorCode == "invalid_horizon");
    }

    [Theory]
    [InlineData(3)]
    [InlineData(6)]
    [InlineData(12)]
    public void Projection_AllowedHorizons_Pass(int horizon)
    {
        var result = new GetSalaryProjectionQueryValidator(time)
            .Validate(new GetSalaryProjectionQuery("2024-01", "2024-06", SalaryBasis.Gross, horizon));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void PurchasingPower_UsesSamePeriodRules()
    {
        var result = new GetPurchasingPowerQueryValidator(time)
            .Validate(new GetPurchasingPowerQuery("2024-06", "2024-01"));

        Assert.Contains(result.Errors, error => error.ErrorCode == "invalid_period");
    }

    [Fact]
    public void Series_UsesSamePeriodRules()
    {
        var result = new GetSalarySeriesQueryValidator(time)
            .Validate(new GetSalarySeriesQuery("2024-01", "2024-07"));

        Assert.Contains(result.Errors, error => error.ErrorCode == "future_competence");
    }

    [Fact]
    public void Insights_UsesSamePeriodRules()
    {
        var result = new GetSalaryInsightsQueryValidator(time)
            .Validate(new GetSalaryInsightsQuery("2014-01", "2024-06"));

        Assert.Contains(result.Errors, error => error.ErrorCode == "period_too_long");
    }

    [Fact]
    public void Summary_NullPeriod_Passes()
    {
        var result = new GetSalaryIntelligenceSummaryQueryValidator(time)
            .Validate(new GetSalaryIntelligenceSummaryQuery(null, null));

        Assert.True(result.IsValid);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
