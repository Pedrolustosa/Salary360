using Salary360.Domain.Enums;

namespace Salary360.Application.Expenses;

public sealed record ExpenseCategoryDto(
    Guid Id,
    string? Name,
    string? SystemKey,
    CategoryColor ColorKey,
    CategoryIcon IconKey,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
