using Salary360.Application.Interfaces.Messaging;

namespace Salary360.Application.Salaries.Commands;

public sealed record CreateSalaryCommand(
    int Year,
    int Month,
    decimal GrossAmount,
    string? Notes,
    IReadOnlyList<SalaryItemInputDto> Items) : ICommand<SalaryRecordDto>;
