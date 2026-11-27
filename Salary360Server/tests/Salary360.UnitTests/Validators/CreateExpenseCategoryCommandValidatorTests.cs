using Salary360.Application.Expenses.Categories.Commands;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;

namespace Salary360.UnitTests.Validators;

public sealed class CreateExpenseCategoryCommandValidatorTests
{
    private readonly CreateExpenseCategoryCommandValidator validator = new();

    [Fact]
    public void ValidCommand_Passes()
        => Assert.True(validator.Validate(new CreateExpenseCategoryCommand(
            "Travel",
            CategoryColor.Blue,
            CategoryIcon.Truck)).IsValid);

    [Fact]
    public void BlankName_FailsRequired()
    {
        var result = validator.Validate(new CreateExpenseCategoryCommand(
            "  ",
            CategoryColor.Blue,
            CategoryIcon.Home));
        Assert.Contains(result.Errors, error => error.ErrorCode == "required");
    }

    [Fact]
    public void NameTooLong_FailsMaxLength()
    {
        var result = validator.Validate(new CreateExpenseCategoryCommand(
            new string('N', ExpenseCategory.MaxNameLength + 1),
            CategoryColor.Blue,
            CategoryIcon.Home));
        Assert.Contains(result.Errors, error => error.ErrorCode == "max_length");
    }

    [Fact]
    public void InvalidColor_Fails()
    {
        var result = validator.Validate(new CreateExpenseCategoryCommand(
            "Pets",
            (CategoryColor)999,
            CategoryIcon.Home));
        Assert.Contains(result.Errors, error => error.ErrorCode == "invalid_color");
    }

    [Fact]
    public void InvalidIcon_Fails()
    {
        var result = validator.Validate(new CreateExpenseCategoryCommand(
            "Pets",
            CategoryColor.Blue,
            (CategoryIcon)999));
        Assert.Contains(result.Errors, error => error.ErrorCode == "invalid_icon");
    }
}
