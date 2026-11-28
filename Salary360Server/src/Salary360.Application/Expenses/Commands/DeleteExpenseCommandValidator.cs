using FluentValidation;

namespace Salary360.Application.Expenses.Commands;

public sealed class DeleteExpenseCommandValidator : AbstractValidator<DeleteExpenseCommand>
{
    public DeleteExpenseCommandValidator()
    {
        RuleFor(x => x.Scope)
            .Must(ExpenseValidation.IsValidScope)
            .WithErrorCode("invalid_scope");
    }
}