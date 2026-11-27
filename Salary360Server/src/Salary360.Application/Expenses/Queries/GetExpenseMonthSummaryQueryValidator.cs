using FluentValidation;

namespace Salary360.Application.Expenses.Queries;

public sealed class GetExpenseMonthSummaryQueryValidator : AbstractValidator<GetExpenseMonthSummaryQuery>
{
    public GetExpenseMonthSummaryQueryValidator()
    {
        RuleFor(x => x.Year)
            .InclusiveBetween(1900, 2100)
            .WithErrorCode("invalid_year");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .WithErrorCode("invalid_month");
    }
}
