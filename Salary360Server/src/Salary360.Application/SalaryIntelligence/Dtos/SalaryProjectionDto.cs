
namespace Salary360.Application.SalaryIntelligence.Dtos;

public sealed record ProjectionScenarioDto(
    string Name,
    decimal Value,
    decimal? RealValue);

public sealed record ProjectionPointDto(
    int Year,
    int Month,
    decimal? Actual,
    decimal? Conservative,
    decimal? Likely,
    decimal? Optimistic);

public sealed record SalaryProjectionDto(
    bool HasEnoughData,
    int RequiredRecords,
    int RecordCount,
    CompetenceDto? From,
    CompetenceDto? To,
    string? Method,
    int? SampleSize,
    int? HorizonMonths,
    string? DisclaimerCode,
    string? InflationCriterion,
    IReadOnlyList<ProjectionScenarioDto> Scenarios,
    IReadOnlyList<ProjectionPointDto> Series);
