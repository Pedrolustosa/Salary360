using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.SalaryIntelligence.Enums;
using Salary360.Application.SalaryIntelligence.Dtos;

namespace Salary360.Application.SalaryIntelligence.Queries;

public sealed record GetSalarySeriesQuery(
    string? From,
    string? To,
    SalaryBasis Basis = SalaryBasis.Net) : IQuery<SalarySeriesDto>;
