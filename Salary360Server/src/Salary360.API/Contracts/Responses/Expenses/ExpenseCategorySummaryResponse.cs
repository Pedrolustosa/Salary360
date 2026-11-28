using Salary360.Domain.Enums;

namespace Salary360.API.Contracts.Responses.Expenses;

public sealed record ExpenseCategorySummaryResponse(
    Guid CategoryId,
    string? Name,
    string? SystemKey,
    CategoryColor ColorKey,
    CategoryIcon IconKey,
    decimal Total,
    decimal PercentOfTotal);
