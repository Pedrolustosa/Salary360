using Salary360.Domain.Common;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;

namespace Salary360.UnitTests.Domain;

public sealed class ExpenseCategoryTests
{
    [Fact]
    public void Create_CustomWithoutName_ThrowsRequired()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new ExpenseCategory(Guid.NewGuid(), null, null, CategoryColor.Blue, CategoryIcon.Home));

        Assert.Equal("required", exception.Code);
    }

    [Fact]
    public void Create_NameTooLong_ThrowsMaxLength()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new ExpenseCategory(
                Guid.NewGuid(),
                new string('A', ExpenseCategory.MaxNameLength + 1),
                null,
                CategoryColor.Blue,
                CategoryIcon.Home));

        Assert.Equal("max_length", exception.Code);
    }

    [Fact]
    public void SystemCategory_CanRename()
    {
        var category = new ExpenseCategory(
            Guid.NewGuid(),
            null,
            "housing",
            CategoryColor.Blue,
            CategoryIcon.Home);

        category.Update("Moradia", CategoryColor.Teal, CategoryIcon.Home);

        Assert.Equal("Moradia", category.Name);
        Assert.Equal("housing", category.SystemKey);
        Assert.Equal(CategoryColor.Teal, category.ColorKey);
    }

    [Fact]
    public void SystemCategory_ClearingName_IsAllowed()
    {
        var category = new ExpenseCategory(
            Guid.NewGuid(),
            "Custom Housing",
            "housing",
            CategoryColor.Blue,
            CategoryIcon.Home);

        category.Update(null, CategoryColor.Blue, CategoryIcon.Home);

        Assert.Null(category.Name);
    }
}