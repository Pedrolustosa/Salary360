using Salary360.Domain.Enums;

namespace Salary360.Application.Expenses;

public sealed record ExpenseListItemDto(
    Guid Id,
    Guid CategoryId,
    string? CategoryName,
    string? CategorySystemKey,
    CategoryColor CategoryColorKey,
    CategoryIcon CategoryIconKey,
    string Name,
    ExpenseType Type,
    decimal Amount,
    int Year,
    int Month,
    int? DueDay,
    bool IsPaid,
    DateTime? PaidAtUtc,
    Guid? SeriesId);
