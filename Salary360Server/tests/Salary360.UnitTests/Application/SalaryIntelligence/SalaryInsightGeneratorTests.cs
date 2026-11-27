using Salary360.Application.SalaryIntelligence.Enums;
using Salary360.Application.SalaryIntelligence.Models;
using Salary360.Application.SalaryIntelligence.Services;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;

namespace Salary360.UnitTests.Application.SalaryIntelligence;

public sealed class SalaryInsightGeneratorTests
{
    private readonly SalaryInsightGenerator generator = new();
    private readonly Guid userId = Guid.NewGuid();

    [Fact]
    public void Generate_Empty_ReturnsStablePeriod()
    {
        var insights = generator.Generate([], null, SalaryBasis.Net);

        Assert.Single(insights);
        Assert.Equal("stable_period", insights[0].Code);
        Assert.Equal(InsightSeverity.Info, insights[0].Severity);
    }

    [Fact]
    public void Generate_FlatSeries_IncludesBestWorst()
    {
        var records = new[]
        {
            Record(2024, 1, 5000m, []),
            Record(2024, 2, 5000m, []),
            Record(2024, 3, 5000m, [])
        };

        var insights = generator.Generate(records, null, SalaryBasis.Net);

        Assert.Contains(insights, i => i.Code == "best_month");
        Assert.Contains(insights, i => i.Code == "worst_month");
        Assert.DoesNotContain(insights, i => i.Code is "salary_increase" or "salary_decrease");
    }

    [Fact]
    public void Generate_NetIncreaseAboveThreshold_SalaryIncrease()
    {
        var records = new[]
        {
            Record(2024, 1, 5000m, []),
            Record(2024, 2, 5400m, [])
        };

        var insights = generator.Generate(records, null, SalaryBasis.Net);

        var increase = Assert.Single(insights, i => i.Code == "salary_increase");
        Assert.Equal(InsightSeverity.Positive, increase.Severity);
        Assert.Equal(8m, increase.Params["percent"]);
        Assert.Equal("2024-02", increase.ReferenceCompetence);
    }

    [Fact]
    public void Generate_NetDecreaseAboveThreshold_SalaryDecrease()
    {
        var records = new[]
        {
            Record(2024, 1, 5000m, []),
            Record(2024, 2, 4500m, [])
        };

        var insights = generator.Generate(records, null, SalaryBasis.Net);

        var decrease = Assert.Single(insights, i => i.Code == "salary_decrease");
        Assert.Equal(InsightSeverity.Warning, decrease.Severity);
        Assert.Equal(10m, decrease.Params["percent"]);
    }

    [Fact]
    public void Generate_BelowThreshold_NoSalaryChangeInsight()
    {
        var records = new[]
        {
            Record(2024, 1, 5000m, []),
            Record(2024, 2, 5200m, [])
        };

        var insights = generator.Generate(records, null, SalaryBasis.Net);

        Assert.DoesNotContain(insights, i => i.Code is "salary_increase" or "salary_decrease");
    }

    [Fact]
    public void Generate_RealLoss_WhenThresholdCrossed()
    {
        var records = new[]
        {
            Record(2024, 1, 5000m, []),
            Record(2024, 2, 5000m, [])
        };
        var purchasingPower = new PurchasingPowerResult(
            true, false, null, 0m, 8m, -7.41m, -370.5m, []);

        var insights = generator.Generate(records, purchasingPower, SalaryBasis.Net);

        var loss = Assert.Single(insights, i => i.Code == "real_loss");
        Assert.Equal(InsightSeverity.Warning, loss.Severity);
        Assert.Equal(7.41m, loss.Params["percent"]);
    }

    [Fact]
    public void Generate_RealGain_WhenThresholdCrossed()
    {
        var records = new[]
        {
            Record(2024, 1, 5000m, []),
            Record(2024, 2, 5500m, [])
        };
        var purchasingPower = new PurchasingPowerResult(
            true, false, null, 10m, 1m, 8.91m, 445.5m, []);

        var insights = generator.Generate(records, purchasingPower, SalaryBasis.Net);

        var gain = Assert.Single(insights, i => i.Code == "real_gain");
        Assert.Equal(InsightSeverity.Positive, gain.Severity);
        Assert.Equal(8.91m, gain.Params["percent"]);
    }

    [Fact]
    public void Generate_DeductionsRise()
    {
        var records = new[]
        {
            Record(2024, 1, 5000m, [("INSS", SalaryItemType.Deduction, 200m)]),
            Record(2024, 2, 5000m, [("INSS", SalaryItemType.Deduction, 600m)])
        };

        var insights = generator.Generate(records, null, SalaryBasis.Net);

        var rise = Assert.Single(insights, i => i.Code == "deductions_rise");
        Assert.Equal(InsightSeverity.Warning, rise.Severity);
        Assert.Equal(8m, rise.Params["percent"]);
    }

    [Fact]
    public void Generate_DeductionsDrop()
    {
        var records = new[]
        {
            Record(2024, 1, 5000m, [("INSS", SalaryItemType.Deduction, 600m)]),
            Record(2024, 2, 5000m, [("INSS", SalaryItemType.Deduction, 200m)])
        };

        var insights = generator.Generate(records, null, SalaryBasis.Net);

        var drop = Assert.Single(insights, i => i.Code == "deductions_drop");
        Assert.Equal(InsightSeverity.Positive, drop.Severity);
        Assert.Equal(8m, drop.Params["percent"]);
    }

    [Fact]
    public void Generate_BenefitsRise()
    {
        var records = new[]
        {
            Record(2024, 1, 5000m, [("VR", SalaryItemType.Benefit, 100m)]),
            Record(2024, 2, 5000m, [("VR", SalaryItemType.Benefit, 500m)])
        };

        var insights = generator.Generate(records, null, SalaryBasis.Net);

        var rise = Assert.Single(insights, i => i.Code == "benefits_rise");
        Assert.Equal(InsightSeverity.Warning, rise.Severity);
        Assert.Equal(8m, rise.Params["percent"]);
    }

    [Fact]
    public void Generate_BenefitsDrop()
    {
        var records = new[]
        {
            Record(2024, 1, 5000m, [("VR", SalaryItemType.Benefit, 500m)]),
            Record(2024, 2, 5000m, [("VR", SalaryItemType.Benefit, 100m)])
        };

        var insights = generator.Generate(records, null, SalaryBasis.Net);

        var drop = Assert.Single(insights, i => i.Code == "benefits_drop");
        Assert.Equal(InsightSeverity.Positive, drop.Severity);
        Assert.Equal(8m, drop.Params["percent"]);
    }

    [Fact]
    public void Generate_NetRatioChange()
    {
        var records = new[]
        {
            Record(2024, 1, 5000m, [("INSS", SalaryItemType.Deduction, 100m)]),
            Record(2024, 2, 5000m, [("INSS", SalaryItemType.Deduction, 600m)])
        };

        var insights = generator.Generate(records, null, SalaryBasis.Net);

        var ratio = Assert.Single(insights, i => i.Code == "net_ratio_change");
        Assert.Equal(InsightSeverity.Warning, ratio.Severity);
        Assert.Equal(10m, ratio.Params["percent"]);
    }

    [Fact]
    public void Generate_BestAndWorstMonths()
    {
        var records = new[]
        {
            Record(2024, 1, 4000m, []),
            Record(2024, 2, 6000m, []),
            Record(2024, 3, 5000m, [])
        };

        var insights = generator.Generate(records, null, SalaryBasis.Net);

        var best = Assert.Single(insights, i => i.Code == "best_month");
        var worst = Assert.Single(insights, i => i.Code == "worst_month");
        Assert.Equal(6000m, best.Params["amount"]);
        Assert.Equal("2024-02", best.Params["competence"]);
        Assert.Equal(4000m, worst.Params["amount"]);
        Assert.Equal("2024-01", worst.Params["competence"]);
    }

    [Fact]
    public void Generate_InflationUnavailable_SkipsRealInsights()
    {
        var records = new[]
        {
            Record(2024, 1, 5000m, []),
            Record(2024, 2, 5000m, [])
        };
        var purchasingPower = new PurchasingPowerResult(
            false, false, null, 0m, null, -10m, null, []);

        var insights = generator.Generate(records, purchasingPower, SalaryBasis.Net);

        Assert.DoesNotContain(insights, i => i.Code is "real_loss" or "real_gain");
    }

    [Fact]
    public void Generate_LimitsToEightInsights()
    {
        var records = new[]
        {
            Record(2024, 1, 5000m,
            [
                ("VR", SalaryItemType.Benefit, 100m),
                ("INSS", SalaryItemType.Deduction, 100m)
            ]),
            Record(2024, 2, 6000m,
            [
                ("VR", SalaryItemType.Benefit, 800m),
                ("INSS", SalaryItemType.Deduction, 800m)
            ])
        };
        var purchasingPower = new PurchasingPowerResult(
            true, false, null, 20m, 1m, 18m, 900m, []);

        var insights = generator.Generate(records, purchasingPower, SalaryBasis.Net);

        Assert.True(insights.Count <= 8);
    }

    private SalaryRecord Record(
        int year,
        int month,
        decimal gross,
        IEnumerable<(string Name, SalaryItemType Type, decimal Amount)> items)
        => new(userId, year, month, gross, null, items);
}
