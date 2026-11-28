using Salary360.Domain.Common;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;

namespace Salary360.UnitTests.Domain;

public sealed class ExpenseTests
{
    [Fact]
    public void Create_PaidAtWithoutIsPaid_Throws()
    {
        var exception = Assert.Throws<DomainException>(() =>
            Create(isPaid: false, paidAtUtc: DateTime.UtcNow));

        Assert.Equal("invalid", exception.Code);
    }

    [Fact]
    public void Create_IsPaidWithoutPaidAt_SetsPaidAtUtc()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var expense = Create(isPaid: true, paidAtUtc: null);

        Assert.True(expense.IsPaid);
        Assert.NotNull(expense.PaidAtUtc);
        Assert.True(expense.PaidAtUtc >= before);
    }

    [Fact]
    public void SetPaid_False_ClearsPaidAt()
    {
        var expense = Create(isPaid: true, paidAtUtc: DateTime.UtcNow);
        expense.SetPaid(false);

        Assert.False(expense.IsPaid);
        Assert.Null(expense.PaidAtUtc);
    }

    [Fact]
    public void Amount_Zero_Throws()
    {
        var exception = Assert.Throws<DomainException>(() =>
            Create(amount: 0m));

        Assert.Equal("must_be_positive", exception.Code);
    }

    [Fact]
    public void Amount_AboveMax_Throws()
    {
        var exception = Assert.Throws<DomainException>(() =>
            Create(amount: Expense.MaxAmount + 0.01m));

        Assert.Equal("must_be_positive", exception.Code);
    }

    [Fact]
    public void Amount_AtMax_IsAccepted()
    {
        var expense = Create(amount: Expense.MaxAmount);
        Assert.Equal(Expense.MaxAmount, expense.Amount);
    }

    [Fact]
    public void ResolveDueDay_ClampsToFebruary()
    {
        Assert.Equal(28, Expense.ResolveDueDay(2023, 2, 31));
        Assert.Equal(29, Expense.ResolveDueDay(2024, 2, 31));
    }

    [Fact]
    public void ResolveDueDay_Null_ReturnsZero()
        => Assert.Equal(0, Expense.ResolveDueDay(2024, 1, null));

    private static Expense Create(
        decimal amount = 100m,
        bool isPaid = false,
        DateTime? paidAtUtc = null)
        => new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test",
            ExpenseType.Variable,
            amount,
            2024,
            3,
            10,
            isPaid,
            null,
            null,
            paidAtUtc);
}