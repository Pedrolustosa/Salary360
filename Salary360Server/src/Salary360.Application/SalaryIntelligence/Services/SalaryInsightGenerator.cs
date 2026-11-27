using Salary360.Application.SalaryIntelligence.Enums;
using Salary360.Application.SalaryIntelligence.Interfaces;
using Salary360.Application.SalaryIntelligence.Models;
using Salary360.Domain.Entities;

namespace Salary360.Application.SalaryIntelligence.Services;

public sealed class SalaryInsightGenerator : ISalaryInsightGenerator
{
    public IReadOnlyList<InsightResult> Generate(
        IReadOnlyList<SalaryRecord> records,
        PurchasingPowerResult? purchasingPower,
        SalaryBasis basis)
    {
        var ordered = records
            .OrderBy(record => record.Year)
            .ThenBy(record => record.Month)
            .ToList();

        if (ordered.Count == 0)
            return [Stable()];

        var insights = new List<InsightResult>();
        var first = ordered[0];
        var last = ordered[^1];
        var threshold = SalaryInsightThresholds.PercentThreshold * 100m;

        for (var index = 1; index < ordered.Count; index++)
        {
            var previous = ordered[index - 1];
            var current = ordered[index];
            var change = PercentMath.PercentChange(previous.NetAmount, current.NetAmount);
            if (change is null)
                continue;

            if (change >= threshold)
            {
                insights.Add(new InsightResult(
                    "salary_increase",
                    InsightSeverity.Positive,
                    Params(("percent", change.Value)),
                    CompetenceLabel(current)));
            }
            else if (change <= -threshold)
            {
                insights.Add(new InsightResult(
                    "salary_decrease",
                    InsightSeverity.Warning,
                    Params(("percent", Math.Abs(change.Value))),
                    CompetenceLabel(current)));
            }
        }

        var periodNet = PercentMath.PercentChange(first.NetAmount, last.NetAmount);
        if (periodNet is not null && periodNet.Value >= threshold && insights.All(i => i.Code != "salary_increase"))
        {
            insights.Add(new InsightResult(
                "salary_increase",
                InsightSeverity.Positive,
                Params(("percent", periodNet.Value)),
                CompetenceLabel(last)));
        }
        else if (periodNet is not null && periodNet.Value <= -threshold && insights.All(i => i.Code != "salary_decrease"))
        {
            insights.Add(new InsightResult(
                "salary_decrease",
                InsightSeverity.Warning,
                Params(("percent", Math.Abs(periodNet.Value))),
                CompetenceLabel(last)));
        }

        if (purchasingPower is { InflationAvailable: true, RealGainPercent: not null })
        {
            var real = purchasingPower.RealGainPercent.Value;
            if (real <= -threshold)
            {
                insights.Add(new InsightResult(
                    "real_loss",
                    InsightSeverity.Warning,
                    Params(("percent", Math.Abs(real))),
                    CompetenceLabel(last)));
            }
            else if (real >= threshold)
            {
                insights.Add(new InsightResult(
                    "real_gain",
                    InsightSeverity.Positive,
                    Params(("percent", real)),
                    CompetenceLabel(last)));
            }
        }

        AddShareInsight(
            insights,
            first.TotalDeductions,
            first.GrossAmount,
            last.TotalDeductions,
            last.GrossAmount,
            "deductions_rise",
            "deductions_drop",
            last);

        AddShareInsight(
            insights,
            first.TotalBenefits,
            first.GrossAmount,
            last.TotalBenefits,
            last.GrossAmount,
            "benefits_rise",
            "benefits_drop",
            last);

        var firstRatio = PercentMath.Ratio(first.NetAmount, first.GrossAmount);
        var lastRatio = PercentMath.Ratio(last.NetAmount, last.GrossAmount);
        if (firstRatio is not null && lastRatio is not null)
        {
            var delta = lastRatio.Value - firstRatio.Value;
            if (Math.Abs(delta) >= threshold)
            {
                insights.Add(new InsightResult(
                    "net_ratio_change",
                    delta < 0 ? InsightSeverity.Warning : InsightSeverity.Info,
                    Params(("percent", PercentMath.RoundPercent(Math.Abs(delta)))),
                    CompetenceLabel(last)));
            }
        }

        var best = ordered.MaxBy(record => record.NetAmount)!;
        var worst = ordered.MinBy(record => record.NetAmount)!;
        insights.Add(new InsightResult(
            "best_month",
            InsightSeverity.Positive,
            Params(("amount", best.NetAmount), ("competence", CompetenceLabel(best))),
            CompetenceLabel(best)));
        insights.Add(new InsightResult(
            "worst_month",
            InsightSeverity.Info,
            Params(("amount", worst.NetAmount), ("competence", CompetenceLabel(worst))),
            CompetenceLabel(worst)));

        var ranked = insights
            .OrderByDescending(insight => SeverityRank(insight.Severity))
            .ThenByDescending(insight => Relevance(insight))
            .Take(8)
            .ToList();

        return ranked.Count == 0 ? [Stable()] : ranked;
    }

    private static void AddShareInsight(
        List<InsightResult> insights,
        decimal firstPart,
        decimal firstGross,
        decimal lastPart,
        decimal lastGross,
        string riseCode,
        string dropCode,
        SalaryRecord last)
    {
        var firstShare = PercentMath.Ratio(firstPart, firstGross);
        var lastShare = PercentMath.Ratio(lastPart, lastGross);
        if (firstShare is null || lastShare is null)
            return;

        var delta = lastShare.Value - firstShare.Value;
        var threshold = SalaryInsightThresholds.PercentThreshold * 100m;
        if (delta >= threshold)
        {
            insights.Add(new InsightResult(
                riseCode,
                InsightSeverity.Warning,
                Params(("percent", PercentMath.RoundPercent(delta))),
                CompetenceLabel(last)));
        }
        else if (delta <= -threshold)
        {
            insights.Add(new InsightResult(
                dropCode,
                InsightSeverity.Positive,
                Params(("percent", PercentMath.RoundPercent(Math.Abs(delta)))),
                CompetenceLabel(last)));
        }
    }

    private static InsightResult Stable()
        => new("stable_period", InsightSeverity.Info, new Dictionary<string, object>(), null);

    private static string CompetenceLabel(SalaryRecord record)
        => $"{record.Year:D4}-{record.Month:D2}";

    private static IReadOnlyDictionary<string, object> Params(params (string Key, object Value)[] values)
        => values.ToDictionary(pair => pair.Key, pair => pair.Value);

    private static int SeverityRank(InsightSeverity severity) => severity switch
    {
        InsightSeverity.Warning => 3,
        InsightSeverity.Positive => 2,
        _ => 1
    };

    private static decimal Relevance(InsightResult insight)
    {
        if (insight.Params.TryGetValue("percent", out var percent) && percent is decimal value)
            return Math.Abs(value);
        return 0m;
    }
}
