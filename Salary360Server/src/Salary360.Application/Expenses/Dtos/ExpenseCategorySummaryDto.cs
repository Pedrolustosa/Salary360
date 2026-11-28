using Salary360.Domain.Enums;
namespace Salary360.Application.Expenses.Dtos;

public sealed record ExpenseCategorySummaryDto(
    Guid CategoryId,
    string? Name,
    string? SystemKey,
    CategoryColor ColorKey,
    CategoryIcon IconKey,
    decimal Total,
    decimal PercentOfTotal);
