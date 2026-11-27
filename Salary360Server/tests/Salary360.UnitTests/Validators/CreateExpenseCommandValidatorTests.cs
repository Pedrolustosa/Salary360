using Salary360.Application.Expenses.Commands;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;

namespace Salary360.UnitTests.Validators;

public sealed class CreateExpenseCommandValidatorTests
{
    private readonly CreateExpenseCommandValidator validator = new(TimeProvider.System);

    [Fact]
    public void ValidCommand_Passes()
    {
        var now = DateTime.UtcNow;
        Assert.True(validator.Validate(Valid(now.Year, now.Month)).IsValid);
    }

    [Fact]
    public void BlankName_FailsRequired()
    {
        var now = DateTime.UtcNow;
        var result = validator.Validate(Valid(now.Year, now.Month) with { Name = "  " });
        Assert.Contains(result.Errors, error => error.ErrorCode == "required");
    }

    [Fact]
    public void NameTooLong_FailsMaxLength()
    {
        var now = DateTime.UtcNow;
        var result = validator.Validate(Valid(now.Year, now.Month) with
        {
            Name = new string('x', Expense.MaxNameLength + 1)
        });
        Assert.Contains(result.Errors, error => error.ErrorCode == "max_length");
    }

    [Fact]
    public void NonPositiveAmount_Fails()
    {
        var now = DateTime.UtcNow;
        var result = validator.Validate(Valid(now.Year, now.Month) with { Amount = 0m });
        Assert.Contains(result.Errors, error => error.ErrorCode == "must_be_positive");
    }

    [Fact]
    public void InvalidMonth_Fails()
    {
        var result = validator.Validate(Valid(2024, 13));
        Assert.Contains(result.Errors, error => error.ErrorCode == "invalid_month");
    }

    [Fact]
    public void FutureBeyondLimit_Fails()
    {
        var far = DateTime.UtcNow.AddMonths(Expense.MaxFutureMonths + 2);
        var result = validator.Validate(Valid(far.Year, far.Month));
        Assert.Contains(result.Errors, error => error.ErrorCode == "future_competence_limit");
    }

    [Fact]
    public void MonthlyWithInvalidCount_Fails()
    {
        var now = DateTime.UtcNow;
        var result = validator.Validate(Valid(now.Year, now.Month) with
        {
            Recurrence = ExpenseRecurrence.Monthly,
            RecurrenceCount = 1
        });
        Assert.Contains(result.Errors, error => error.ErrorCode == "invalid_recurrence_count");
    }

    [Fact]
    public void NoneWithCount_Fails()
    {
        var now = DateTime.UtcNow;
        var result = validator.Validate(Valid(now.Year, now.Month) with
        {
            Recurrence = ExpenseRecurrence.None,
            RecurrenceCount = 3
        });
        Assert.Contains(result.Errors, error => error.ErrorCode == "invalid_recurrence_count");
    }

    [Fact]
    public void InvalidDueDay_Fails()
    {
        var now = DateTime.UtcNow;
        var result = validator.Validate(Valid(now.Year, now.Month) with { DueDay = 0 });
        Assert.Contains(result.Errors, error => error.ErrorCode == "invalid_due_day");
    }

    private static CreateExpenseCommand Valid(int year, int month)
        => new(
            Guid.NewGuid(),
            "Rent",
            ExpenseType.Fixed,
            1000m,
            year,
            month,
            5,
            false,
            null,
            ExpenseRecurrence.None,
            null);
}
