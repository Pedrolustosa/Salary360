using FluentValidation;
using Salary360.Domain.Enums;

namespace Salary360.Application.Expenses.Commands;

public sealed class CreateExpenseCommandValidator : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseCommandValidator(TimeProvider time)
    {
        ExpenseCommandRules.ApplyShared(
            this,
            x => x.CategoryId,
            x => x.Name,
            x => x.Type,
            x => x.Amount,
            x => x.Year,
            x => x.Month,
            x => x.DueDay,
            x => x.Notes,
            time);

        RuleFor(x => x.Recurrence)
            .Must(ExpenseValidation.IsValidRecurrence)
            .WithErrorCode("invalid");

        RuleFor(x => x.RecurrenceCount)
            .InclusiveBetween(2, 60)
            .WithErrorCode("invalid_recurrence_count")
            .When(x => x.Recurrence == ExpenseRecurrence.Monthly);

        RuleFor(x => x.RecurrenceCount)
            .Empty()
            .WithErrorCode("invalid_recurrence_count")
            .When(x => x.Recurrence == ExpenseRecurrence.None);
    }
}