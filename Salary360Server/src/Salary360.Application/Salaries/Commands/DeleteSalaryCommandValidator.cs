using FluentValidation;

namespace Salary360.Application.Salaries.Commands;

public sealed class DeleteSalaryCommandValidator : AbstractValidator<DeleteSalaryCommand>
{
    public DeleteSalaryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode("required");
    }
}