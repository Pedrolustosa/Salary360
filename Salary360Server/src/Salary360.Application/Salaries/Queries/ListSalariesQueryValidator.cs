using FluentValidation;

namespace Salary360.Application.Salaries.Queries;

public sealed class ListSalariesQueryValidator : AbstractValidator<ListSalariesQuery>
{
    public ListSalariesQueryValidator()
    {
        RuleFor(x => x.Year!)
            .InclusiveBetween(1900, 2100)
            .WithErrorCode("invalid_year")
            .When(x => x.Year.HasValue);

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithErrorCode("must_be_positive");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithErrorCode("must_be_positive");
    }
}