using Salary360.Application.Common;
using Salary360.Application.Common.Exceptions;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Domain.Enums;
using Salary360.Domain.Interfaces;

namespace Salary360.Application.Expenses.Commands;

public sealed class DeleteExpenseCommandHandler(
    IExpenseRepository expenseRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<DeleteExpenseCommand, Unit>
{
    public async Task<Unit> HandleAsync(DeleteExpenseCommand request, CancellationToken cancellationToken = default)
    {
        var expense = await expenseRepository.GetByIdAsync(request.Id, cancellationToken);
        if (expense is null || expense.UserId != currentUser.UserId)
            throw new NotFoundException("Expense", request.Id);

        IReadOnlyList<Domain.Entities.Expense> targets;
        if (expense.SeriesId is null || request.Scope == ExpenseChangeScope.This)
        {
            targets = [expense];
        }
        else if (request.Scope == ExpenseChangeScope.All)
        {
            targets = await expenseRepository.ListBySeriesAsync(
                expense.UserId, expense.SeriesId.Value, cancellationToken);
        }
        else
        {
            targets = await expenseRepository.ListBySeriesFromAsync(
                expense.UserId, expense.SeriesId.Value, expense.Year, expense.Month, cancellationToken);
        }

        expenseRepository.RemoveRange(targets);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
