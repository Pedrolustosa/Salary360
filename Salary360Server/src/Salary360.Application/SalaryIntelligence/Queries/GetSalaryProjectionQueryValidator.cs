using FluentValidation;

namespace Salary360.Application.SalaryIntelligence.Queries;

public sealed class GetSalaryProjectionQueryValidator : AbstractValidator<GetSalaryProjectionQuery>
{
    private static readonly int[] AllowedHorizons = [3, 6, 12];

    public GetSalaryProjectionQueryValidator(TimeProvider time)
    {
        SalaryAnalysisFilterRules.Apply(
            this,
            query => query.From,
            query => query.To,
            query => query.Basis,
            time);

        RuleFor(query => query.HorizonMonths)
            .Must(horizon => AllowedHorizons.Contains(horizon))
            .WithErrorCode("invalid_horizon");
    }
}
