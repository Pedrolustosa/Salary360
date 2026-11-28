using Salary360.Application.SalaryIntelligence.Enums;
using Salary360.Application.SalaryIntelligence.Services;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;

namespace Salary360.UnitTests.Application.SalaryIntelligence;

public sealed class PurchasingPowerCalculatorTests
{
    private readonly PurchasingPowerCalculator calculator = new();
    private readonly Guid userId = Guid.NewGuid();

    [Fact]
    public void Calculate_IpcaAccumulated_IsIndexRatioMinusOne()
    {
        var records = new[]
        {
            Record(2024, 1, 5000m, []),
            Record(2024, 3, 5000m, [])
        };
        var indices = new Dictionary<(int, int), decimal>
        {
            [(2024, 1)] = 100m,
            [(2024, 3)] = 101.5m
        };

        var result = calculator.Calculate(records, SalaryBasis.Gross, indices, false);

        Assert.True(result.InflationAvailable);
        Assert.Equal(1.5m, result.IpcaAccumulatedPercent);
        Assert.Equal(0m, result.NominalChangePercent);
        Assert.True(result.RealGainPercent < 0m);
        Assert.True(result.RealGainAbsolute < 0m);
    }

    [Fact]
    public void Calculate_RealGain_PositiveWhenSalaryOutpacesInflation()
    {
        var records = new[]
        {
            Record(2024, 1, 5000m, []),
            Record(2024, 2, 5500m, [])
        };
        var indices = new Dictionary<(int, int), decimal>
        {
            [(2024, 1)] = 100m,
            [(2024, 2)] = 101m
        };

        var result = calculator.Calculate(records, SalaryBasis.Gross, indices, false);

        Assert.Equal(10m, result.NominalChangePercent);
        Assert.Equal(1m, result.IpcaAccumulatedPercent);
        Assert.True(result.RealGainPercent > 0m);
        Assert.True(result.RealGainAbsolute > 0m);
    }

    [Fact]
    public void Calculate_RealGain_ZeroWhenSalaryMatchesInflation()
    {
        var records = new[]
        {
            Record(2024, 1, 5000m, []),
            Record(2024, 2, 5050m, [])
        };
        var indices = new Dictionary<(int, int), decimal>
        {
            [(2024, 1)] = 100m,
            [(2024, 2)] = 101m
        };

        var result = calculator.Calculate(records, SalaryBasis.Gross, indices, false);

        Assert.Equal(1m, result.NominalChangePercent);
        Assert.Equal(1m, result.IpcaAccumulatedPercent);
        Assert.Equal(0m, result.RealGainPercent);
        Assert.Equal(0m, result.RealGainAbsolute);
    }

    [Fact]
    public void Calculate_UsesNetBasis()
    {
        var records = new[]
        {
            Record(2024, 1, 5000m, [("INSS", SalaryItemType.Deduction, 500m)]),
            Record(2024, 2, 5000m, [("INSS", SalaryItemType.Deduction, 400m)])
        };
        var indices = new Dictionary<(int, int), decimal>
        {
            [(2024, 1)] = 100m,
            [(2024, 2)] = 100m
        };

        var result = calculator.Calculate(records, SalaryBasis.Net, indices, false);

        Assert.Equal(4500m, result.Series[0].Actual);
        Assert.Equal(4600m, result.Series[1].Actual);
        Assert.True(result.NominalChangePercent > 0m);
    }

    [Fact]
    public void Calculate_MissingStartIndex_InflationUnavailable()
    {
        var records = new[]
        {
            Record(2024, 1, 5000m, []),
            Record(2024, 2, 5500m, [])
        };
        var indices = new Dictionary<(int, int), decimal>
        {
            [(2024, 2)] = 101m
        };

        var result = calculator.Calculate(records, SalaryBasis.Gross, indices, true);

        Assert.False(result.InflationAvailable);
        Assert.True(result.IsStale);
        Assert.Null(result.IpcaAccumulatedPercent);
        Assert.Null(result.RealGainPercent);
        Assert.Null(result.Coverage);
        Assert.Equal(10m, result.NominalChangePercent);
        Assert.Null(result.Series[0].InflationAdjusted);
    }

    [Fact]
    public void Calculate_MissingEndIndex_UsesLatestAvailableAsPartial()
    {
        var records = new[]
        {
            Record(2024, 1, 5000m, []),
            Record(2024, 3, 5000m, [])
        };
        var indices = new Dictionary<(int, int), decimal>
        {
            [(2024, 1)] = 100m,
            [(2024, 2)] = 101m
        };

        var result = calculator.Calculate(records, SalaryBasis.Gross, indices, false);

        Assert.True(result.InflationAvailable);
        Assert.NotNull(result.Coverage);
        Assert.True(result.Coverage!.IsPartial);
        Assert.Equal(2024, result.Coverage.ToYear);
        Assert.Equal(2, result.Coverage.ToMonth);
        Assert.Equal(1m, result.IpcaAccumulatedPercent);
    }

    [Fact]
    public void Calculate_Series_InflationAdjustedFromStartIndex()
    {
        var records = new[]
        {
            Record(2024, 1, 5000m, []),
            Record(2024, 2, 5000m, [])
        };
        var indices = new Dictionary<(int, int), decimal>
        {
            [(2024, 1)] = 100m,
            [(2024, 2)] = 110m
        };

        var result = calculator.Calculate(records, SalaryBasis.Gross, indices, false);

        Assert.Equal(5000m, result.Series[0].InflationAdjusted);
        Assert.Equal(5500m, result.Series[1].InflationAdjusted);
    }

    [Fact]
    public void Calculate_ZeroStartValue_NominalPercentIsZero()
    {
        var records = new[]
        {
            Record(2024, 1, 1000m, [("Tax", SalaryItemType.Deduction, 1000m)]),
            Record(2024, 2, 1000m, [("Tax", SalaryItemType.Deduction, 900m)])
        };
        var indices = new Dictionary<(int, int), decimal>
        {
            [(2024, 1)] = 100m,
            [(2024, 2)] = 100m
        };

        var result = calculator.Calculate(records, SalaryBasis.Net, indices, false);

        Assert.Equal(0m, result.NominalChangePercent);
        Assert.True(result.InflationAvailable);
    }

    private SalaryRecord Record(
        int year,
        int month,
        decimal gross,
        IEnumerable<(string Name, SalaryItemType Type, decimal Amount)> items)
        => new(userId, year, month, gross, null, items);
}