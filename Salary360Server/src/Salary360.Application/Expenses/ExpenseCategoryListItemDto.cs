using Salary360.Domain.Enums;

namespace Salary360.Application.Expenses;

public sealed record ExpenseCategoryListItemDto(
    Guid Id,
    string? Name,
    string? SystemKey,
    CategoryColor ColorKey,
    CategoryIcon IconKey,
    int ExpenseCount,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
