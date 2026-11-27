using FluentValidation;
using Salary360.Application.Salaries.Enums;

namespace Salary360.Application.Salaries.Queries;

public sealed class GetSalaryEvolutionQueryValidator : AbstractValidator<GetSalaryEvolutionQuery>
{
    public GetSalaryEvolutionQueryValidator()
    {
        RuleFor(x => x.Mode).IsInEnum().WithErrorCode("invalid");

        RuleFor(x => x.Month!)
            .InclusiveBetween(1, 12)
            .WithErrorCode("invalid_month")
            .When(x => x.Mode == SalaryEvolutionMode.Month);

        RuleFor(x => x.Month)
            .Null()
            .When(x => x.Mode == SalaryEvolutionMode.Year);

        RuleFor(x => x.FromYear)
            .InclusiveBetween(1900, 2100)
            .WithErrorCode("invalid_year");

        RuleFor(x => x.ToYear)
            .InclusiveBetween(1900, 2100)
            .WithErrorCode("invalid_year")
            .GreaterThanOrEqualTo(x => x.FromYear)
            .WithErrorCode("invalid_year");

        RuleFor(x => x)
            .Must(x => x.ToYear - x.FromYear <= 100)
            .WithErrorCode("invalid_year")
            .WithMessage("Year range cannot exceed 100 years.");
    }
}
