using System.Linq.Expressions;
using FluentValidation;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;

namespace Salary360.Application.Expenses;

public static class ExpenseCommandRules
{
    public static void ApplyShared<T>(
        AbstractValidator<T> validator,
        Expression<Func<T, Guid>> categoryId,
        Expression<Func<T, string>> name,
        Expression<Func<T, ExpenseType>> type,
        Expression<Func<T, decimal>> amount,
        Expression<Func<T, int>> year,
        Expression<Func<T, int>> month,
        Expression<Func<T, int?>> dueDay,
        Expression<Func<T, string?>> notes,
        TimeProvider time)
    {
        validator.RuleFor(categoryId)
            .NotEmpty()
            .WithErrorCode("required");

        validator.RuleFor(name)
            .Cascade(CascadeMode.Stop)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithErrorCode("required")
            .Must(value => value.Trim().Length <= Expense.MaxNameLength)
            .WithErrorCode("max_length");

        validator.RuleFor(type)
            .Must(ExpenseValidation.IsValidType)
            .WithErrorCode("invalid");

        validator.RuleFor(amount)
            .GreaterThan(0)
            .WithErrorCode("must_be_positive")
            .LessThanOrEqualTo(Expense.MaxAmount)
            .WithErrorCode("must_be_positive");

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
                return ExpenseValidation.IsWithinFutureLimit(yearValue, monthValue, time);
            })
            .WithErrorCode("future_competence_limit")
            .OverridePropertyName(((MemberExpression)year.Body).Member.Name);

        validator.RuleFor(dueDay)
            .Must(value => value is null or (>= 1 and <= 31))
            .WithErrorCode("invalid_due_day");

        validator.RuleFor(notes)
            .Must(value => value is null || value.Length <= Expense.MaxNotesLength)
            .WithErrorCode("max_length");
    }
}