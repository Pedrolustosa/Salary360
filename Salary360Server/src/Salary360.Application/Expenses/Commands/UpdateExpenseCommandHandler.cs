using FluentValidation;
using FluentValidation.Results;
using Salary360.Application.Common.Exceptions;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Domain.Enums;
using Salary360.Domain.Interfaces;

namespace Salary360.Application.Expenses.Commands;

public sealed class UpdateExpenseCommandHandler(
    IExpenseRepository expenseRepository,
    IExpenseCategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<UpdateExpenseCommand, ExpenseDto>
{
    public async Task<ExpenseDto> HandleAsync(
        UpdateExpenseCommand request, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;
        var expense = await expenseRepository.GetByIdAsync(request.Id, cancellationToken);
        if (expense is null || expense.UserId != userId)
            throw new NotFoundException("Expense", request.Id);

        var category = await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null || category.UserId != userId)
            throw new NotFoundException("ExpenseCategory", request.CategoryId, "category_not_found");

        var competenceChanged = expense.Year != request.Year || expense.Month != request.Month;
        if (competenceChanged && request.Scope != ExpenseChangeScope.This)
        {
            throw new ValidationException([
                new ValidationFailure("scope", "Competence can only change with Scope This.")
                {
                    ErrorCode = "invalid_scope"
                }
            ]);
        }

        if (competenceChanged)
        {
            var existing = await expenseRepository.CountInMonthAsync(userId, request.Year, request.Month, cancellationToken);
            if (existing >= Domain.Entities.Expense.MaxPerUserPerMonth)
                throw new ConflictException("Too many expenses in the month.", "too_many_expenses_in_month");
        }

        var targets = await ResolveTargetsAsync(expense, request.Scope, cancellationToken);
        foreach (var target in targets)
        {
            if (target.Id == expense.Id)
            {
                target.Update(
                    category.Id,
                    request.Name,
                    request.Type,
                    request.Amount,
                    request.Year,
                    request.Month,
                    request.DueDay,
                    request.IsPaid,
                    request.Notes);
            }
            else
            {
                target.UpdateWithoutCompetence(
                    category.Id,
                    request.Name,
                    request.Type,
                    request.Amount,
                    request.DueDay,
                    request.IsPaid,
                    request.Notes);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return expense.ToDto();
    }

    private async Task<IReadOnlyList<Domain.Entities.Expense>> ResolveTargetsAsync(
        Domain.Entities.Expense expense,
        ExpenseChangeScope scope,
        CancellationToken cancellationToken)
    {
        if (expense.SeriesId is null || scope == ExpenseChangeScope.This)
            return [expense];

        if (scope == ExpenseChangeScope.All)
            return await expenseRepository.ListBySeriesAsync(
                expense.UserId, expense.SeriesId.Value, cancellationToken);

        return await expenseRepository.ListBySeriesFromAsync(
            expense.UserId, expense.SeriesId.Value, expense.Year, expense.Month, cancellationToken);
    }
}
