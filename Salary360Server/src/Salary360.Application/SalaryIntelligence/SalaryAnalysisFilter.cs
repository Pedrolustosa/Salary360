using Salary360.Application.SalaryIntelligence.Enums;

namespace Salary360.Application.SalaryIntelligence;

public sealed record SalaryAnalysisFilter(
    string? From,
    string? To,
    SalaryBasis Basis = SalaryBasis.Net);
