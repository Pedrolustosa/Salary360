using Salary360.Domain.Enums;

namespace Salary360.API.Contracts.Responses.Expenses;

public sealed record ExpenseCategoryListItemResponse(
    Guid Id,
    string? Name,
    string? SystemKey,
    CategoryColor ColorKey,
    CategoryIcon IconKey,
    int ExpenseCount,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
