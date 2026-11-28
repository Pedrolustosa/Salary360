using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Salaries.Dtos;

namespace Salary360.Application.Salaries.Commands;

public sealed record UpdateSalaryCommand(
    Guid Id,
    int Year,
    int Month,
    decimal GrossAmount,
    string? Notes,
    IReadOnlyList<SalaryItemInputDto> Items) : ICommand<SalaryRecordDto>;
