using Salary360.Application.Common.Exceptions;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Domain.Interfaces;

namespace Salary360.Application.Expenses.Commands;

public sealed class SetExpensePaidCommandHandler(
    IExpenseRepository expenseRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<SetExpensePaidCommand, ExpenseDto>
{
    public async Task<ExpenseDto> HandleAsync(
        SetExpensePaidCommand request, CancellationToken cancellationToken = default)
    {
        var expense = await expenseRepository.GetByIdAsync(request.Id, cancellationToken);
        if (expense is null || expense.UserId != currentUser.UserId)
            throw new NotFoundException("Expense", request.Id);

        expense.SetPaid(request.IsPaid);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return expense.ToDto();
    }
}
