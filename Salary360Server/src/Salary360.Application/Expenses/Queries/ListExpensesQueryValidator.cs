using FluentValidation;

namespace Salary360.Application.Expenses.Queries;

public sealed class ListExpensesQueryValidator : AbstractValidator<ListExpensesQuery>
{
    public ListExpensesQueryValidator()
    {
        RuleFor(x => x.Year)
            .InclusiveBetween(1900, 2100)
            .WithErrorCode("invalid_year");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .WithErrorCode("invalid_month");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithErrorCode("must_be_positive");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithErrorCode("must_be_positive");
    }
}
