using FluentValidation;
using Salary360.Domain.Entities;
namespace Salary360.Application.Salaries.Dtos;

public sealed class SalaryItemInputDtoValidator : AbstractValidator<SalaryItemInputDto>
{
    public SalaryItemInputDtoValidator()
    {
        RuleFor(x => x.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithErrorCode("required")
            .MaximumLength(SalaryRecord.MaxItemNameLength)
            .WithErrorCode("max_length");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithErrorCode("invalid");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithErrorCode("must_be_positive");
    }
}