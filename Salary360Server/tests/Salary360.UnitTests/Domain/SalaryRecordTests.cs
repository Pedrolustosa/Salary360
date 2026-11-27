using Salary360.Domain.Common;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;

namespace Salary360.UnitTests.Domain;

public sealed class SalaryRecordTests
{
    [Fact]
    public void NetAmount_SubtractsDeductionsAndAddsBenefits()
    {
        var record = Create(5000m, [("VR", SalaryItemType.Benefit, 500m), ("INSS", SalaryItemType.Deduction, 400.555m)]);

        Assert.Equal(500m, record.TotalBenefits);
        Assert.Equal(400.56m, record.TotalDeductions);
        Assert.Equal(5099.44m, record.NetAmount);
    }

    [Fact]
    public void Round_UsesBankersRounding()
    {
        Assert.Equal(1.24m, Money.Round(1.235m));
        Assert.Equal(1.24m, Money.Round(1.245m));
    }

    [Fact]
    public void Create_MoreThan50Items_Throws()
    {
        var items = Enumerable.Range(1, 51)
            .Select(index => ($"Item {index}", SalaryItemType.Benefit, 1m));

        var exception = Assert.Throws<DomainException>(() => Create(1000m, items));
        Assert.Equal("too_many_items", exception.Code);
    }

    [Fact]
    public void Update_ReplacesItems()
    {
        var record = Create(3000m, [("Old", SalaryItemType.Benefit, 10m)]);
        record.Update(2024, 5, 3000m, null, [("New", SalaryItemType.Deduction, 20m)]);

        Assert.Single(record.Items);
        Assert.Equal("New", record.Items.First().Name);
        Assert.Equal(SalaryItemType.Deduction, record.Items.First().Type);
    }

    [Fact]
    public void NetAmount_CanBeNegative()
    {
        var record = Create(100m, [("Tax", SalaryItemType.Deduction, 250m)]);
        Assert.Equal(-150m, record.NetAmount);
    }

    private static SalaryRecord Create(
        decimal gross,
        IEnumerable<(string Name, SalaryItemType Type, decimal Amount)> items)
        => new(Guid.NewGuid(), 2024, 3, gross, null, items);
}
