using Salary360.Application.SalaryIntelligence.Enums;

namespace Salary360.Application.SalaryIntelligence.Dtos;

public sealed record SalaryInsightDto(
    string Code,
    InsightSeverity Severity,
    IReadOnlyDictionary<string, object> Params,
    string? ReferenceCompetence);

public sealed record SalaryInsightsDto(
    bool HasEnoughData,
    int RequiredRecords,
    int RecordCount,
    CompetenceDto? From,
    CompetenceDto? To,
    IReadOnlyList<SalaryInsightDto> Insights);
