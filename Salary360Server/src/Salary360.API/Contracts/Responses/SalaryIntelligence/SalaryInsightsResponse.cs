using Salary360.Application.SalaryIntelligence.Enums;

namespace Salary360.API.Contracts.Responses.SalaryIntelligence;

public sealed record SalaryInsightResponse(
    string Code,
    InsightSeverity Severity,
    IReadOnlyDictionary<string, object> Params,
    string? ReferenceCompetence);

public sealed record SalaryInsightsResponse(
    bool HasEnoughData,
    int RequiredRecords,
    int RecordCount,
    CompetenceResponse? From,
    CompetenceResponse? To,
    IReadOnlyList<SalaryInsightResponse> Insights);
