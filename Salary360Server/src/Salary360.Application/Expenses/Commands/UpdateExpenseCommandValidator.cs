using FluentValidation;

namespace Salary360.Application.Expenses.Commands;

public sealed class UpdateExpenseCommandValidator : AbstractValidator<UpdateExpenseCommand>
{
    public UpdateExpenseCommandValidator(TimeProvider time)
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

        RuleFor(x => x.Scope)
            .Must(ExpenseValidation.IsValidScope)
            .WithErrorCode("invalid_scope");
    }
}