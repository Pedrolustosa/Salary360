using Salary360.Domain.Enums;
namespace Salary360.Application.Salaries.Dtos;

public sealed record SalaryItemDto(
    Guid Id,
    string Name,
    SalaryItemType Type,
    decimal Amount,
    int Order);
