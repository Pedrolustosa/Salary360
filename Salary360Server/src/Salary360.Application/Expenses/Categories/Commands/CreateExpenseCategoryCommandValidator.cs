using FluentValidation;
using Salary360.Domain.Entities;

namespace Salary360.Application.Expenses.Categories.Commands;

public sealed class CreateExpenseCategoryCommandValidator : AbstractValidator<CreateExpenseCategoryCommand>
{
    public CreateExpenseCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithErrorCode("required")
            .Must(name => name.Trim().Length <= ExpenseCategory.MaxNameLength)
            .WithErrorCode("max_length");

        RuleFor(x => x.ColorKey)
            .Must(ExpenseValidation.IsValidColor)
            .WithErrorCode("invalid_color");

        RuleFor(x => x.IconKey)
            .Must(ExpenseValidation.IsValidIcon)
            .WithErrorCode("invalid_icon");
    }
}
