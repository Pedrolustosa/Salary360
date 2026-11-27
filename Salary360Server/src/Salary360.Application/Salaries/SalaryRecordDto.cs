namespace Salary360.Application.Salaries;

public sealed record SalaryRecordDto(
    Guid Id,
    int Year,
    int Month,
    decimal GrossAmount,
    decimal TotalBenefits,
    decimal TotalDeductions,
    decimal NetAmount,
    string? Notes,
    IReadOnlyList<SalaryItemDto> Items,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
