using Salary360.Application.SalaryIntelligence;
using Salary360.Application.SalaryIntelligence.Enums;
using Salary360.Application.SalaryIntelligence.Services;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;

namespace Salary360.UnitTests.Application.SalaryIntelligence;

public sealed class SalaryProjectionCalculatorTests
{
    private readonly SalaryProjectionCalculator calculator = new();
    private readonly Guid userId = Guid.NewGuid();

    [Fact]
    public void Calculate_FewerThanSixRecords_ReturnsNull()
    {
        var records = Enumerable.Range(1, 5)
            .Select(month => Record(2024, month, 5000m + month * 100m))
            .ToList();

        var result = calculator.Calculate(records, SalaryBasis.Gross, 6, null);

        Assert.Null(result);
    }

    [Fact]
    public void Calculate_GrowingSeries_LikelyAboveLast()
    {
        var records = Enumerable.Range(1, 6)
            .Select(month => Record(2024, month, 4000m + month * 200m))
            .ToList();

        var result = calculator.Calculate(records, SalaryBasis.Gross, 3, null);

        Assert.NotNull(result);
        var likely = result!.Scenarios.Single(s => s.Name == "likely").Value;
        Assert.True(likely > records[^1].GrossAmount);
        Assert.Equal("ols_linear", result.Method);
        Assert.Equal(6, result.SampleSize);
        Assert.Equal(3, result.HorizonMonths);
        Assert.Equal("projection_estimate", result.DisclaimerCode);
        Assert.Null(result.InflationCriterion);
    }

    [Fact]
    public void Calculate_DecliningSeries_LikelyBelowLast()
    {
        var records = Enumerable.Range(1, 6)
            .Select(month => Record(2024, month, 8000m - month * 200m))
            .ToList();

        var result = calculator.Calculate(records, SalaryBasis.Gross, 3, null);

        Assert.NotNull(result);
        var likely = result!.Scenarios.Single(s => s.Name == "likely").Value;
        Assert.True(likely < records[^1].GrossAmount);
    }

    [Fact]
    public void Calculate_ConstantSeries_LikelyNearLast()
    {
        var records = Enumerable.Range(1, 6)
            .Select(month => Record(2024, month, 5000m))
            .ToList();

        var result = calculator.Calculate(records, SalaryBasis.Gross, 6, null);

        Assert.NotNull(result);
        var likely = result!.Scenarios.Single(s => s.Name == "likely").Value;
        Assert.Equal(5000m, likely);
        Assert.Equal(5000m, result.Scenarios.Single(s => s.Name == "conservative").Value);
        Assert.Equal(5000m, result.Scenarios.Single(s => s.Name == "optimistic").Value);
    }

    [Fact]
    public void Calculate_ScenariosNeverNegative()
    {
        var records = new[]
        {
            Record(2024, 1, 600m),
            Record(2024, 2, 500m),
            Record(2024, 3, 400m),
            Record(2024, 4, 300m),
            Record(2024, 5, 200m),
            Record(2024, 6, 100m)
        };

        var result = calculator.Calculate(records, SalaryBasis.Gross, 12, null);

        Assert.NotNull(result);
        Assert.All(result!.Scenarios, scenario => Assert.True(scenario.Value >= 0m));
        Assert.All(
            result.Series.Where(point => point.Likely is not null),
            point =>
            {
                Assert.True(point.Conservative >= 0m);
                Assert.True(point.Likely >= 0m);
                Assert.True(point.Optimistic >= 0m);
            });
    }

    [Fact]
    public void Calculate_WithIndices_IncludesRealLikelyAndCriterion()
    {
        var records = Enumerable.Range(1, 6)
            .Select(month => Record(2024, month, 4000m + month * 100m))
            .ToList();
        var indices = new Dictionary<(int, int), decimal>();
        var index = 100m;
        for (var month = 1; month <= 6; month++)
        {
            indices[(2024, month)] = index;
            index *= 1.01m;
        }

        var result = calculator.Calculate(records, SalaryBasis.Gross, 6, indices);

        Assert.NotNull(result);
        Assert.Equal("average_ipca_last_12_months", result!.InflationCriterion);
        var likely = result.Scenarios.Single(s => s.Name == "likely");
        Assert.NotNull(likely.RealValue);
        Assert.True(likely.RealValue < likely.Value);
        Assert.True(likely.RealValue >= 0m);
    }

    [Fact]
    public void Calculate_UsesLastTwelveAsSample()
    {
        var records = Enumerable.Range(0, 15)
            .Select(offset =>
            {
                var competence = new Competence(2023, 1).AddMonths(offset);
                return Record(competence.Year, competence.Month, 5000m + offset * 50m);
            })
            .ToList();

        var result = calculator.Calculate(records, SalaryBasis.Gross, 3, null);

        Assert.NotNull(result);
        Assert.Equal(12, result!.SampleSize);
        Assert.Equal(12, result.Series.Count(point => point.Actual is not null));
        Assert.Equal(3, result.Series.Count(point => point.Likely is not null));
    }

    [Fact]
    public void Calculate_UsesNetBasis()
    {
        var records = Enumerable.Range(1, 6)
            .Select(month => Record(
                2024,
                month,
                5000m,
                [("INSS", SalaryItemType.Deduction, 1000m - month * 50m)]))
            .ToList();

        var result = calculator.Calculate(records, SalaryBasis.Net, 3, null);

        Assert.NotNull(result);
        var historical = result!.Series.Where(point => point.Actual is not null).ToList();
        Assert.Equal(records[0].NetAmount, historical[0].Actual);
        Assert.Equal(records[^1].NetAmount, historical[^1].Actual);
    }

    private SalaryRecord Record(
        int year,
        int month,
        decimal gross,
        IEnumerable<(string Name, SalaryItemType Type, decimal Amount)>? items = null)
        => new(userId, year, month, gross, null, items ?? []);
}
