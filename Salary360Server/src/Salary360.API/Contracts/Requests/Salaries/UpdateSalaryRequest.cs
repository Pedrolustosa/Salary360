using Salary360.Application.Salaries.Dtos;

namespace Salary360.API.Contracts.Requests.Salaries;

public sealed record UpdateSalaryRequest(
    int Year,
    int Month,
    decimal GrossAmount,
    string? Notes,
    IReadOnlyList<SalaryItemInputDto> Items);
