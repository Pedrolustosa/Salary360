using Salary360.Application.Common.Exceptions;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Persistence;

namespace Salary360.Application.Expenses.Queries;

public sealed class GetExpenseQueryHandler(
    IExpenseRepository repository,
    ICurrentUser currentUser) : IQueryHandler<GetExpenseQuery, ExpenseDto>
{
    public async Task<ExpenseDto> HandleAsync(GetExpenseQuery request, CancellationToken cancellationToken = default)
    {
        var expense = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (expense is null || expense.UserId != currentUser.UserId)
            throw new NotFoundException("Expense", request.Id);

        return expense.ToDto();
    }
}
