using Salary360.Domain.Enums;
namespace Salary360.Application.Expenses.Dtos;

public sealed record ExpenseDto(
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
