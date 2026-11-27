using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Persistence;

namespace Salary360.Application.Expenses.Queries;

public sealed class GetExpenseSeriesQueryHandler(
    IExpenseRepository repository,
    ICurrentUser currentUser) : IQueryHandler<GetExpenseSeriesQuery, IReadOnlyList<ExpenseSeriesPointDto>>
{
    public Task<IReadOnlyList<ExpenseSeriesPointDto>> HandleAsync(
        GetExpenseSeriesQuery request, CancellationToken cancellationToken = default)
        => repository.GetSeriesAsync(
            currentUser.UserId,
            request.FromYear,
            request.FromMonth,
            request.ToYear,
            request.ToMonth,
            cancellationToken);
}
