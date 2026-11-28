using System.Linq.Expressions;
using FluentValidation;
using Salary360.Application.Salaries.Dtos;
using Salary360.Domain.Entities;

namespace Salary360.Application.Salaries;

public static class SalaryCommandRules
{
    public static void ApplyShared<T>(
        AbstractValidator<T> validator,
        Expression<Func<T, int>> year,
        Expression<Func<T, int>> month,
        Expression<Func<T, decimal>> grossAmount,
        Expression<Func<T, string?>> notes,
        Expression<Func<T, IReadOnlyList<SalaryItemInputDto>>> items,
        TimeProvider time)
    {
        validator.RuleFor(year)
            .InclusiveBetween(1900, 2100)
            .WithErrorCode("invalid_year");

        validator.RuleFor(month)
            .InclusiveBetween(1, 12)
            .WithErrorCode("invalid_month");

        validator.RuleFor(x => x)
            .Must(instance =>
            {
                var yearValue = year.Compile()(instance);
                var monthValue = month.Compile()(instance);
                return SalaryValidation.IsNotFutureCompetence(yearValue, monthValue, time);
})
            .WithErrorCode("future_competence")
            .OverridePropertyName(((MemberExpression)year.Body).Member.Name);

        validator.RuleFor(grossAmount)
            .GreaterThan(0)
            .WithErrorCode("must_be_positive")
            .LessThanOrEqualTo(SalaryValidation.MaxGrossAmount)
            .WithErrorCode("must_be_positive");

        validator.RuleFor(notes)
            .Must(value => value is null || value.Length <= SalaryRecord.MaxNotesLength)
            .WithErrorCode("max_length");

        validator.RuleFor(items)
            .NotNull()
            .WithErrorCode("required")
            .Must(list => list.Count <= SalaryRecord.MaxItems)
            .WithErrorCode("too_many_items")
            .Must(SalaryValidation.HasUniqueItemNames)
            .WithErrorCode("duplicate_item_name");
}
}