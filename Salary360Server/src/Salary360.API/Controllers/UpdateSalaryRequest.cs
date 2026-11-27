using Salary360.Application.Salaries;

namespace Salary360.API.Controllers;

public sealed record UpdateSalaryRequest(
    int Year,
    int Month,
    decimal GrossAmount,
    string? Notes,
    IReadOnlyList<SalaryItemInputDto> Items);
