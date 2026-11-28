using FluentValidation;

namespace Salary360.Application.Salaries.Queries;

public sealed class GetSalaryQueryValidator : AbstractValidator<GetSalaryQuery>
{
    public GetSalaryQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode("required");
    }
}