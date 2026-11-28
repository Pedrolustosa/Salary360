using FluentValidation;

namespace Salary360.Application.SalaryIntelligence.Queries;

public sealed class GetPurchasingPowerQueryValidator : AbstractValidator<GetPurchasingPowerQuery>
{
    public GetPurchasingPowerQueryValidator(TimeProvider time)
        => SalaryAnalysisFilterRules.Apply(
            this,
            query => query.From,
            query => query.To,
            query => query.Basis,
            time);
}