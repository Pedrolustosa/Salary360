namespace Salary360.API.Contracts.Responses.SalaryIntelligence;

public sealed record ProjectionScenarioResponse(
    string Name,
    decimal Value,
    decimal? RealValue);

public sealed record ProjectionPointResponse(
    int Year,
    int Month,
    decimal? Actual,
    decimal? Conservative,
    decimal? Likely,
    decimal? Optimistic);

public sealed record SalaryProjectionResponse(
    bool HasEnoughData,
    int RequiredRecords,
    int RecordCount,
    CompetenceResponse? From,
    CompetenceResponse? To,
    string? Method,
    int? SampleSize,
    int? HorizonMonths,
    string? DisclaimerCode,
    string? InflationCriterion,
    IReadOnlyList<ProjectionScenarioResponse> Scenarios,
    IReadOnlyList<ProjectionPointResponse> Series);
