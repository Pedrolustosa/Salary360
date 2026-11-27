using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.SalaryIntelligence.Dtos;
using Salary360.Application.SalaryIntelligence.Enums;

namespace Salary360.Application.SalaryIntelligence.Queries;

public sealed record GetSalaryInsightsQuery(
    string? From,
    string? To,
    SalaryBasis Basis = SalaryBasis.Net) : IQuery<SalaryInsightsDto>;
