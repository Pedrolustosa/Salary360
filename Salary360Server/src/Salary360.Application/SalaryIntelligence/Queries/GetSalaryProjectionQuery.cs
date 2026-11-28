using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.SalaryIntelligence.Enums;
using Salary360.Application.SalaryIntelligence.Dtos;

namespace Salary360.Application.SalaryIntelligence.Queries;

public sealed record GetSalaryProjectionQuery(
    string? From,
    string? To,
    SalaryBasis Basis = SalaryBasis.Net,
    int HorizonMonths = 6) : IQuery<SalaryProjectionDto>;
