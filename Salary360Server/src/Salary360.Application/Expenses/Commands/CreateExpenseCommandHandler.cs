using Salary360.Application.Common.Exceptions;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;
using Salary360.Domain.Interfaces;

namespace Salary360.Application.Expenses.Commands;

public sealed class CreateExpenseCommandHandler(
    IExpenseRepository expenseRepository,
    IExpenseCategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider time) : ICommandHandler<CreateExpenseCommand, IReadOnlyList<ExpenseDto>>
{
    public async Task<IReadOnlyList<ExpenseDto>> HandleAsync(
        CreateExpenseCommand request, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;
        var category = await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null || category.UserId != userId)
            throw new NotFoundException("ExpenseCategory", request.CategoryId, "category_not_found");

        var count = request.Recurrence == ExpenseRecurrence.Monthly
            ? request.RecurrenceCount!.Value
            : 1;

        var seriesId = count > 1 ? Guid.NewGuid() : (Guid?)null;
        var created = new List<Expense>(count);

        for (var offset = 0; offset < count; offset++)
        {
            var (year, month) = ExpenseValidation.AddMonths(request.Year, request.Month, offset);
            if (!ExpenseValidation.IsWithinFutureLimit(year, month, time))
                throw new ConflictException("Future competence exceeds the allowed limit.", "future_competence_limit");

            var existing = await expenseRepository.CountInMonthAsync(userId, year, month, cancellationToken);
            if (existing >= Expense.MaxPerUserPerMonth)
                throw new ConflictException("Too many expenses in the month.", "too_many_expenses_in_month");

            var expense = new Expense(
                userId,
                category.Id,
                request.Name,
                request.Type,
                request.Amount,
                year,
                month,
                request.DueDay,
                offset == 0 && request.IsPaid,
                request.Notes,
                seriesId,
                offset == 0 && request.IsPaid ? DateTime.UtcNow : null);

            created.Add(expense);
        }

        expenseRepository.AddRange(created);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return created.Select(expense => expense.ToDto()).ToList();
    }
}
