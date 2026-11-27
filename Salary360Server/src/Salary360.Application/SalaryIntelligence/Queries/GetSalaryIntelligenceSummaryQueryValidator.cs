using FluentValidation;

namespace Salary360.Application.SalaryIntelligence.Queries;

public sealed class GetSalaryIntelligenceSummaryQueryValidator : AbstractValidator<GetSalaryIntelligenceSummaryQuery>
{
    public GetSalaryIntelligenceSummaryQueryValidator(TimeProvider time)
        => SalaryAnalysisFilterRules.Apply(
            this,
            query => query.From,
            query => query.To,
            query => query.Basis,
            time);
}
