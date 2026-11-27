using FluentValidation;

namespace Salary360.Application.Expenses.Commands;

public sealed class CopyExpensesCommandValidator : AbstractValidator<CopyExpensesCommand>
{
    public CopyExpensesCommandValidator(TimeProvider time)
    {
        RuleFor(x => x.FromYear)
            .InclusiveBetween(1900, 2100)
            .WithErrorCode("invalid_year");

        RuleFor(x => x.FromMonth)
            .InclusiveBetween(1, 12)
            .WithErrorCode("invalid_month");

        RuleFor(x => x.ToYear)
            .InclusiveBetween(1900, 2100)
            .WithErrorCode("invalid_year");

        RuleFor(x => x.ToMonth)
            .InclusiveBetween(1, 12)
            .WithErrorCode("invalid_month");

        RuleFor(x => x)
            .Must(command => ExpenseValidation.IsWithinFutureLimit(command.ToYear, command.ToMonth, time))
            .WithErrorCode("future_competence_limit")
            .OverridePropertyName("toYear");
    }
}
