using Salary360.Domain.Enums;
namespace Salary360.Application.Salaries.Dtos;

public sealed record SalaryItemInputDto(
    string Name,
    SalaryItemType Type,
    decimal Amount);
