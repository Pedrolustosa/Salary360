using FluentValidation;
using Salary360.Domain.Entities;

namespace Salary360.Application.Salaries.Commands;

public sealed class UpdateSalaryCommandValidator : AbstractValidator<UpdateSalaryCommand>
{
    public UpdateSalaryCommandValidator(TimeProvider time)
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode("required");

        RuleFor(x => x.Year)
            .InclusiveBetween(1900, 2100)
            .WithErrorCode("invalid_year");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .WithErrorCode("invalid_month");

        RuleFor(x => x)
            .Must(command => SalaryValidation.IsNotFutureCompetence(command.Year, command.Month, time))
            .WithErrorCode("future_competence")
            .OverridePropertyName("year");

        RuleFor(x => x.GrossAmount)
            .GreaterThan(0)
            .WithErrorCode("must_be_positive")
            .LessThanOrEqualTo(SalaryValidation.MaxGrossAmount)
            .WithErrorCode("must_be_positive");

        RuleFor(x => x.Notes!)
            .MaximumLength(SalaryRecord.MaxNotesLength)
            .WithErrorCode("max_length")
            .When(x => x.Notes is not null);

        RuleFor(x => x.Items)
            .NotNull()
            .WithErrorCode("required")
            .Must(items => items.Count <= SalaryRecord.MaxItems)
            .WithErrorCode("too_many_items")
            .Must(SalaryValidation.HasUniqueItemNames)
            .WithErrorCode("duplicate_item_name");

        RuleForEach(x => x.Items).SetValidator(new SalaryItemInputDtoValidator());
    }
}
