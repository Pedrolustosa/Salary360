namespace Salary360.API.Contracts.Responses.Salaries;

public sealed record SalaryRecordResponse(
    Guid Id,
    int Year,
    int Month,
    decimal GrossAmount,
    decimal TotalBenefits,
    decimal TotalDeductions,
    decimal NetAmount,
    string? Notes,
    IReadOnlyList<SalaryItemResponse> Items,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
