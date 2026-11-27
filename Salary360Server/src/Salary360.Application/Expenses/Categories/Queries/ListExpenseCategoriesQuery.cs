using Salary360.Application.Interfaces.Messaging;

namespace Salary360.Application.Expenses.Categories.Queries;

public sealed record ListExpenseCategoriesQuery : IQuery<IReadOnlyList<ExpenseCategoryListItemDto>>;
