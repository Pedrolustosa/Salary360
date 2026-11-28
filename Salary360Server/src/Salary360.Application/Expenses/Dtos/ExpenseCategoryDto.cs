using Salary360.Domain.Enums;
namespace Salary360.Application.Expenses.Dtos;

public sealed record ExpenseCategoryDto(
    Guid Id,
    string? Name,
    string? SystemKey,
    CategoryColor ColorKey,
    CategoryIcon IconKey,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
