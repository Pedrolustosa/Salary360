using FluentValidation;
using FluentValidation.Results;
using Salary360.Application.Salaries;
using Salary360.Application.SalaryIntelligence.Enums;

namespace Salary360.Application.SalaryIntelligence;

public static class SalaryAnalysisFilterRules
{
    public static void Apply<T>(
        AbstractValidator<T> validator,
        Func<T, string?> fromSelector,
        Func<T, string?> toSelector,
        Func<T, SalaryBasis> basisSelector,
        TimeProvider time)
    {
        validator.RuleFor(x => basisSelector(x))
            .IsInEnum()
            .WithErrorCode("invalid_basis");

        validator.RuleFor(x => x)
            .Custom((instance, context) =>
            {
                var fromRaw = fromSelector(instance);
                var toRaw = toSelector(instance);
                var fromProvided = !string.IsNullOrWhiteSpace(fromRaw);
                var toProvided = !string.IsNullOrWhiteSpace(toRaw);

                if (fromProvided != toProvided)
                {
                    Fail(context, "from", "invalid_period");
                    return;
                }

                if (!fromProvided)
                    return;

                var from = Competence.TryParse(fromRaw);
                var to = Competence.TryParse(toRaw);
                if (from is null || to is null)
                {
                    Fail(context, "from", "invalid_period");
                    return;
                }

                if (from.Value.CompareTo(to.Value) > 0)
                {
                    Fail(context, "from", "invalid_period");
                    return;
                }

                if (Competence.MonthsBetween(from.Value, to.Value) > SalaryAnalysisPeriodResolver.MaxWindowMonths)
                {
                    Fail(context, "to", "period_too_long");
                    return;
                }

                if (!SalaryValidation.IsNotFutureCompetence(from.Value.Year, from.Value.Month, time)
                    || !SalaryValidation.IsNotFutureCompetence(to.Value.Year, to.Value.Month, time))
                {
                    Fail(context, "to", "future_competence");
                }
            });
    }

    private static void Fail<T>(ValidationContext<T> context, string property, string code)
        => context.AddFailure(new ValidationFailure(property, code) { ErrorCode = code });
}
