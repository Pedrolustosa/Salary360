using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Application.Expenses.Dtos;

namespace Salary360.Application.Expenses.Queries;

public sealed class ListExpensesQueryHandler(
    IExpenseRepository repository,
    ICurrentUser currentUser) : IQueryHandler<ListExpensesQuery, PagedExpensesDto>
{
    public async Task<PagedExpensesDto> HandleAsync(
        ListExpensesQuery request, CancellationToken cancellationToken = default)
    {
        var (items, total) = await repository.ListAsync(
            currentUser.GetUserId(),
            request.Year,
            request.Month,
            request.CategoryId,
            request.Type,
            request.IsPaid,
            request.Page,
            request.PageSize,
            cancellationToken);

        return new PagedExpensesDto(items, total, request.Page, request.PageSize);
}
}