using Salary360.Domain.Enums;

namespace Salary360.API.Contracts.Responses.Expenses;

public sealed record ExpenseResponse(
    Guid Id,
    Guid CategoryId,
    string Name,
    ExpenseType Type,
    decimal Amount,
    int Year,
    int Month,
    int? DueDay,
    bool IsPaid,
    DateTime? PaidAtUtc,
    string? Notes,
    Guid? SeriesId,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
