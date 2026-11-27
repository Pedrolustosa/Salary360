using Salary360.Application.SalaryIntelligence.Services;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;

namespace Salary360.UnitTests.Application.SalaryIntelligence;

public sealed class SalaryEvolutionCalculatorTests
{
    private readonly SalaryEvolutionCalculator calculator = new();
    private readonly Guid userId = Guid.NewGuid();

    [Fact]
    public void Calculate_Empty_Throws()
    {
        Assert.Throws<ArgumentException>(() => calculator.Calculate([]));
    }

    [Fact]
    public void Calculate_GrossAndNetAbsoluteAndPercent()
    {
        var records = new[]
        {
            Record(2024, 1, 5000m, [("VR", SalaryItemType.Benefit, 500m), ("INSS", SalaryItemType.Deduction, 400m)]),
            Record(2024, 2, 5500m, [("VR", SalaryItemType.Benefit, 500m), ("INSS", SalaryItemType.Deduction, 400m)])
        };

        var result = calculator.Calculate(records);

        Assert.Equal(500m, result.Gross.Absolute);
        Assert.Equal(10m, result.Gross.Percent);
        Assert.Equal(500m, result.Net.Absolute);
        Assert.Equal(9.8m, result.Net.Percent);
    }

    [Fact]
    public void Calculate_ZeroNetBase_PercentIsNull()
    {
        var records = new[]
        {
            Record(2024, 1, 1000m, [("Tax", SalaryItemType.Deduction, 1000m)]),
            Record(2024, 2, 1000m, [("Tax", SalaryItemType.Deduction, 900m)])
        };

        var result = calculator.Calculate(records);

        Assert.Equal(0m, records[0].NetAmount);
        Assert.Null(result.Net.Percent);
        Assert.Equal(100m, result.Net.Absolute);
        Assert.Null(result.Series[1].NetChangePercent);
    }

    [Fact]
    public void Calculate_SeriesPoint_FirstHasNullChanges()
    {
        var records = new[]
        {
            Record(2024, 1, 4000m, []),
            Record(2024, 2, 4400m, [])
        };

        var result = calculator.Calculate(records);

        Assert.Equal(2, result.Series.Count);
        Assert.Null(result.Series[0].GrossChangeAbsolute);
        Assert.Null(result.Series[0].GrossChangePercent);
        Assert.Null(result.Series[0].NetChangeAbsolute);
        Assert.Null(result.Series[0].NetChangePercent);
        Assert.Equal(400m, result.Series[1].GrossChangeAbsolute);
        Assert.Equal(10m, result.Series[1].GrossChangePercent);
        Assert.Equal(400m, result.Series[1].NetChangeAbsolute);
        Assert.Equal(10m, result.Series[1].NetChangePercent);
    }

    [Fact]
    public void Calculate_OrdersByCompetence()
    {
        var records = new[]
        {
            Record(2024, 3, 3000m, []),
            Record(2024, 1, 1000m, []),
            Record(2024, 2, 2000m, [])
        };

        var result = calculator.Calculate(records);

        Assert.Equal(2000m, result.Gross.Absolute);
        Assert.Equal(2024, result.Series[0].Year);
        Assert.Equal(1, result.Series[0].Month);
        Assert.Equal(3, result.Series[^1].Month);
    }

    [Fact]
    public void Calculate_CompositionSharesAndTotals()
    {
        var records = new[]
        {
            Record(2024, 1, 5000m,
            [
                ("VR", SalaryItemType.Benefit, 500m),
                ("VT", SalaryItemType.Benefit, 250m),
                ("INSS", SalaryItemType.Deduction, 400m)
            ]),
            Record(2024, 2, 5000m,
            [
                ("VR", SalaryItemType.Benefit, 600m),
                ("INSS", SalaryItemType.Deduction, 500m)
            ])
        };

        var result = calculator.Calculate(records);

        Assert.Equal(2, result.FirstBenefits.Count);
        Assert.Equal("VR", result.FirstBenefits[0].Name);
        Assert.Equal(500m, result.FirstBenefits[0].Amount);
        Assert.Equal(10m, result.FirstBenefits[0].ShareOfGrossPercent);
        Assert.Equal(5m, result.FirstBenefits[1].ShareOfGrossPercent);
        Assert.Equal(750m, result.FirstTotalBenefits);
        Assert.Equal(400m, result.FirstTotalDeductions);
        Assert.Single(result.LastBenefits);
        Assert.Equal(12m, result.LastBenefits[0].ShareOfGrossPercent);
        Assert.Equal(600m, result.LastTotalBenefits);
        Assert.Equal(500m, result.LastTotalDeductions);
        Assert.Equal(8m, result.FirstDeductions[0].ShareOfGrossPercent);
    }

    [Fact]
    public void Calculate_UsesBankersRoundingForPercent()
    {
        var records = new[]
        {
            Record(2024, 1, 8000m, []),
            Record(2024, 2, 8098m, [])
        };

        var result = calculator.Calculate(records);

        Assert.Equal(1.22m, result.Gross.Percent);
    }

    private SalaryRecord Record(
        int year,
        int month,
        decimal gross,
        IEnumerable<(string Name, SalaryItemType Type, decimal Amount)> items)
        => new(userId, year, month, gross, null, items);
}
