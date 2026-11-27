using Salary360.Domain.Enums;

namespace Salary360.Application.Expenses;

public sealed record ExpenseCategorySummaryDto(
    Guid CategoryId,
    string? Name,
    string? SystemKey,
    CategoryColor ColorKey,
    CategoryIcon IconKey,
    decimal Total,
    decimal PercentOfTotal);
