using FluentValidation;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;

namespace Salary360.Application.Expenses.Commands;

public sealed class UpdateExpenseCommandValidator : AbstractValidator<UpdateExpenseCommand>
{
    public UpdateExpenseCommandValidator(TimeProvider time)
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithErrorCode("required");

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithErrorCode("required")
            .Must(name => name.Trim().Length <= Expense.MaxNameLength)
            .WithErrorCode("max_length");

        RuleFor(x => x.Type)
            .Must(ExpenseValidation.IsValidType)
            .WithErrorCode("invalid");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithErrorCode("must_be_positive")
            .LessThanOrEqualTo(Expense.MaxAmount)
            .WithErrorCode("must_be_positive");

        RuleFor(x => x.Year)
            .InclusiveBetween(1900, 2100)
            .WithErrorCode("invalid_year");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .WithErrorCode("invalid_month");

        RuleFor(x => x)
            .Must(command => ExpenseValidation.IsWithinFutureLimit(command.Year, command.Month, time))
            .WithErrorCode("future_competence_limit")
            .OverridePropertyName("year");

        RuleFor(x => x.DueDay!)
            .InclusiveBetween(1, 31)
            .WithErrorCode("invalid_due_day")
            .When(x => x.DueDay.HasValue);

        RuleFor(x => x.Notes!)
            .MaximumLength(Expense.MaxNotesLength)
            .WithErrorCode("max_length")
            .When(x => x.Notes is not null);

        RuleFor(x => x.Scope)
            .Must(ExpenseValidation.IsValidScope)
            .WithErrorCode("invalid_scope");
    }
}
