using FluentValidation;

namespace Salary360.Application.Expenses.Queries;

public sealed class GetExpenseSeriesQueryValidator : AbstractValidator<GetExpenseSeriesQuery>
{
    public GetExpenseSeriesQueryValidator()
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
            .Must(IsValidRange)
            .WithErrorCode("invalid")
            .OverridePropertyName("fromYear");

        RuleFor(x => x)
            .Must(WithinMaxMonths)
            .WithErrorCode("invalid")
            .OverridePropertyName("toYear");
    }

    private static bool IsValidRange(GetExpenseSeriesQuery query)
    {
        var from = query.FromYear * 100 + query.FromMonth;
        var to = query.ToYear * 100 + query.ToMonth;
        return from <= to;
    }

    private static bool WithinMaxMonths(GetExpenseSeriesQuery query)
    {
        var from = new DateOnly(query.FromYear, query.FromMonth, 1);
        var to = new DateOnly(query.ToYear, query.ToMonth, 1);
        var months = (to.Year - from.Year) * 12 + (to.Month - from.Month) + 1;
        return months is >= 1 and <= 36;
    }
}