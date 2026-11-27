using Salary360.Domain.Enums;

namespace Salary360.Application.Salaries;

public sealed record SalaryItemInputDto(
    string Name,
    SalaryItemType Type,
    decimal Amount);
