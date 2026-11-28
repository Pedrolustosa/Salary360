using Salary360.Domain.Enums;

namespace Salary360.API.Contracts.Requests.Expenses;

public sealed record CreateExpenseCategoryRequest(
    string Name,
    CategoryColor ColorKey,
    CategoryIcon IconKey);
