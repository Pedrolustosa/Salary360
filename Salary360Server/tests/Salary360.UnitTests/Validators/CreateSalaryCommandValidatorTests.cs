using Salary360.Application.Salaries;
using Salary360.Application.Salaries.Commands;
using Salary360.Application.Salaries.Dtos;
using Salary360.Domain.Enums;

namespace Salary360.UnitTests.Validators;

public sealed class CreateSalaryCommandValidatorTests
{
    private readonly CreateSalaryCommandValidator validator =
        new(TimeProvider.System);

    [Fact]
    public void ValidCommand_Passes()
    {
        var now = DateTime.UtcNow;
        var result = validator.Validate(Valid(now.Year, now.Month));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void FutureCompetence_FailsWithCode()
    {
        var now = DateTime.UtcNow;
        var future = now.AddMonths(1);
        var result = validator.Validate(Valid(future.Year, future.Month));
        Assert.Contains(result.Errors, error => error.ErrorCode == "future_competence");
    }

    [Fact]
    public void InvalidMonth_Fails()
    {
        var result = validator.Validate(Valid(2024, 13));
        Assert.Contains(result.Errors, error => error.ErrorCode == "invalid_month");
    }

    [Fact]
    public void InvalidYear_Fails()
    {
        var result = validator.Validate(Valid(1800, 1));
        Assert.Contains(result.Errors, error => error.ErrorCode == "invalid_year");
    }

    [Fact]
    public void NonPositiveGross_Fails()
    {
        var now = DateTime.UtcNow;
        var command = Valid(now.Year, now.Month) with { GrossAmount = 0 };
        var result = validator.Validate(command);
        Assert.Contains(result.Errors, error => error.ErrorCode == "must_be_positive");
    }

    [Fact]
    public void TooManyItems_Fails()
    {
        var now = DateTime.UtcNow;
        var items = Enumerable.Range(1, 51)
            .Select(index => new SalaryItemInputDto($"Item {index}", SalaryItemType.Benefit, 1m))
            .ToList();
        var result = validator.Validate(Valid(now.Year, now.Month) with { Items = items });
        Assert.Contains(result.Errors, error => error.ErrorCode == "too_many_items");
    }

    [Fact]
    public void DuplicateItemNames_Fails()
    {
        var now = DateTime.UtcNow;
        var items = new List<SalaryItemInputDto>
        {
            new("Vale", SalaryItemType.Benefit, 10m),
            new("vale", SalaryItemType.Deduction, 5m)
        };
        var result = validator.Validate(Valid(now.Year, now.Month) with { Items = items });
        Assert.Contains(result.Errors, error => error.ErrorCode == "duplicate_item_name");
    }

    [Fact]
    public void BlankItemName_FailsRequired()
    {
        var now = DateTime.UtcNow;
        var items = new List<SalaryItemInputDto> { new("  ", SalaryItemType.Benefit, 10m) };
        var result = validator.Validate(Valid(now.Year, now.Month) with { Items = items });
        Assert.Contains(result.Errors, error => error.ErrorCode == "required");
    }

    [Fact]
    public void NotesTooLong_Fails()
    {
        var now = DateTime.UtcNow;
        var result = validator.Validate(Valid(now.Year, now.Month) with { Notes = new string('a', 501) });
        Assert.Contains(result.Errors, error => error.ErrorCode == "max_length");
    }

    private static CreateSalaryCommand Valid(int year, int month)
        => new(year, month, 5000m, "note",
        [
            new SalaryItemInputDto("Benefit", SalaryItemType.Benefit, 100m)
        ]);
}