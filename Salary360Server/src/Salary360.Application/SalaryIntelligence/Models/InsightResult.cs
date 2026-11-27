using Salary360.Application.SalaryIntelligence.Enums;

namespace Salary360.Application.SalaryIntelligence.Models;

public sealed record InsightResult(
    string Code,
    InsightSeverity Severity,
    IReadOnlyDictionary<string, object> Params,
    string? ReferenceCompetence);
