using FluentValidation;
using Salary360.Application.Salaries.Dtos;

namespace Salary360.Application.Salaries.Commands;

public sealed class CreateSalaryCommandValidator : AbstractValidator<CreateSalaryCommand>
{
    public CreateSalaryCommandValidator(TimeProvider time)
    {
        SalaryCommandRules.ApplyShared(
            this,
            x => x.Year,
            x => x.Month,
            x => x.GrossAmount,
            x => x.Notes,
            x => x.Items,
            time);

        RuleForEach(x => x.Items).SetValidator(new SalaryItemInputDtoValidator());
    }
}