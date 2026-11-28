using Salary360.Domain.Enums;

namespace Salary360.API.Contracts.Responses.Expenses;

public sealed record ExpenseCategoryResponse(
    Guid Id,
    string? Name,
    string? SystemKey,
    CategoryColor ColorKey,
    CategoryIcon IconKey,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
