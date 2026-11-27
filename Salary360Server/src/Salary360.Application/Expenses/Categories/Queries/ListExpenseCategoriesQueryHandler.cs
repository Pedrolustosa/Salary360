using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Persistence;

namespace Salary360.Application.Expenses.Categories.Queries;

public sealed class ListExpenseCategoriesQueryHandler(
    IExpenseCategoryRepository repository,
    ICurrentUser currentUser) : IQueryHandler<ListExpenseCategoriesQuery, IReadOnlyList<ExpenseCategoryListItemDto>>
{
    public Task<IReadOnlyList<ExpenseCategoryListItemDto>> HandleAsync(
        ListExpenseCategoriesQuery request, CancellationToken cancellationToken = default)
        => repository.ListWithCountsAsync(currentUser.UserId, cancellationToken);
}
