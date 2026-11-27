namespace Salary360.Application.SalaryIntelligence.Models;

public sealed record ProjectionScenario(
    string Name,
    decimal Value,
    decimal? RealValue);

public sealed record ProjectionPoint(
    int Year,
    int Month,
    decimal? Actual,
    decimal? Conservative,
    decimal? Likely,
    decimal? Optimistic);

public sealed record ProjectionResult(
    string Method,
    int SampleSize,
    int HorizonMonths,
    string DisclaimerCode,
    string? InflationCriterion,
    IReadOnlyList<ProjectionScenario> Scenarios,
    IReadOnlyList<ProjectionPoint> Series);
