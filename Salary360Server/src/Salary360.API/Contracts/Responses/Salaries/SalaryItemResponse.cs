using Salary360.Domain.Enums;

namespace Salary360.API.Contracts.Responses.Salaries;

public sealed record SalaryItemResponse(
    Guid Id,
    string Name,
    SalaryItemType Type,
    decimal Amount,
    int Order);
