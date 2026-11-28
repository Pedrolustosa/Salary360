using Salary360.Application.Common.Exceptions;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Domain.Entities;
using Salary360.Domain.Interfaces;
using Salary360.Application.Expenses.Dtos;

namespace Salary360.Application.Expenses.Commands;

public sealed class CopyExpensesCommandHandler(
    IExpenseRepository expenseRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<CopyExpensesCommand, CopyExpensesResultDto>
{
    public async Task<CopyExpensesResultDto> HandleAsync(
        CopyExpensesCommand request, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.GetUserId();
        var source = await expenseRepository.ListForMonthAsync(
            userId, request.FromYear, request.FromMonth, request.OnlyFixed, cancellationToken);

        var existing = await expenseRepository.ExistingNameCategoryPairsAsync(
            userId, request.ToYear, request.ToMonth, cancellationToken);

        var created = 0;
        var skipped = 0;
        var monthCount = await expenseRepository.CountInMonthAsync(
            userId, request.ToYear, request.ToMonth, cancellationToken);

        var toAdd = new List<Expense>();
        foreach (var expense in source)
        {
            var key = (expense.Name, expense.CategoryId);
            if (existing.Contains(key))
            {
                skipped++;
                continue;
            }

            if (monthCount + toAdd.Count >= Expense.MaxPerUserPerMonth)
                throw new ConflictException("Too many expenses in the month.", "too_many_expenses_in_month");

            toAdd.Add(new Expense(
                userId,
                expense.CategoryId,
                expense.Name,
                expense.Type,
                expense.Amount,
                request.ToYear,
                request.ToMonth,
                expense.DueDay,
                isPaid: false,
                expense.Notes,
                seriesId: null));

            existing.Add(key);
            created++;
        }

        if (toAdd.Count > 0)
        {
            expenseRepository.AddRange(toAdd);
            await unitOfWork.SaveChangesAsync(cancellationToken);
}

        return new CopyExpensesResultDto(created, skipped);
}
}