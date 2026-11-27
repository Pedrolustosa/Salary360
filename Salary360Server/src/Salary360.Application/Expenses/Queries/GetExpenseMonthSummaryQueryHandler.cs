using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Persistence;

namespace Salary360.Application.Expenses.Queries;

public sealed class GetExpenseMonthSummaryQueryHandler(
    IExpenseRepository repository,
    ICurrentUser currentUser) : IQueryHandler<GetExpenseMonthSummaryQuery, ExpenseMonthSummaryDto>
{
    public Task<ExpenseMonthSummaryDto> HandleAsync(
        GetExpenseMonthSummaryQuery request, CancellationToken cancellationToken = default)
        => repository.GetMonthSummaryAsync(currentUser.UserId, request.Year, request.Month, cancellationToken);
}
