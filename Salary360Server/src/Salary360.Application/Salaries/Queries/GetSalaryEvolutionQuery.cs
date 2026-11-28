using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Salaries.Enums;
using Salary360.Application.Salaries.Dtos;

namespace Salary360.Application.Salaries.Queries;

public sealed record GetSalaryEvolutionQuery(
    SalaryEvolutionMode Mode,
    int? Month,
    int FromYear,
    int ToYear) : IQuery<IReadOnlyList<SalaryEvolutionPointDto>>;
