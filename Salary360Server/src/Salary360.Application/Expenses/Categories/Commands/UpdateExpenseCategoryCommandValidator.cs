using FluentValidation;
using Salary360.Domain.Entities;

namespace Salary360.Application.Expenses.Categories.Commands;

public sealed class UpdateExpenseCategoryCommandValidator : AbstractValidator<UpdateExpenseCategoryCommand>
{
    public UpdateExpenseCategoryCommandValidator()
    {
        RuleFor(x => x.Name!)
            .Must(name => name.Trim().Length <= ExpenseCategory.MaxNameLength)
            .WithErrorCode("max_length")
            .When(x => x.Name is not null);

        RuleFor(x => x.ColorKey)
            .Must(ExpenseValidation.IsValidColor)
            .WithErrorCode("invalid_color");

        RuleFor(x => x.IconKey)
            .Must(ExpenseValidation.IsValidIcon)
            .WithErrorCode("invalid_icon");
    }
}