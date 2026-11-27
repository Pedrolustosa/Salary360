using Salary360.Application.SalaryIntelligence.Enums;
using Salary360.Application.SalaryIntelligence.Interfaces;
using Salary360.Application.SalaryIntelligence.Models;
using Salary360.Domain.Common;
using Salary360.Domain.Entities;

namespace Salary360.Application.SalaryIntelligence.Services;

public sealed class SalaryProjectionCalculator : ISalaryProjectionCalculator
{
    private const string Method = "ols_linear";
    private const string Disclaimer = "projection_estimate";
    private const string InflationCriterion = "average_ipca_last_12_months";

    public ProjectionResult? Calculate(
        IReadOnlyList<SalaryRecord> records,
        SalaryBasis basis,
        int horizonMonths,
        IReadOnlyDictionary<(int Year, int Month), decimal>? indices)
    {
        var ordered = records
            .OrderBy(record => record.Year)
            .ThenBy(record => record.Month)
            .ToList();

        if (ordered.Count < 6)
            return null;

        var sample = ordered.TakeLast(Math.Min(12, ordered.Count)).ToList();
        var ys = sample.Select(record => Value(record, basis)).ToList();
        var n = ys.Count;
        var xs = Enumerable.Range(0, n).Select(index => (decimal)index).ToList();

        var sumX = xs.Sum();
        var sumY = ys.Sum();
        var sumXy = xs.Zip(ys, (x, y) => x * y).Sum();
        var sumXx = xs.Sum(x => x * x);
        var denominator = n * sumXx - sumX * sumX;
        var slope = denominator == 0m ? 0m : (n * sumXy - sumX * sumY) / denominator;
        var intercept = (sumY - slope * sumX) / n;

        var residuals = xs.Zip(ys, (x, y) => y - (intercept + slope * x)).ToList();
        var residualMean = residuals.Average();
        var variance = residuals.Sum(residual =>
        {
            var delta = residual - residualMean;
            return delta * delta;
        }) / Math.Max(1, n - 1);
        var stdDev = (decimal)Math.Sqrt((double)variance);

        var last = sample[^1];
        var lastCompetence = new Competence(last.Year, last.Month);
        var xFuture = n - 1 + horizonMonths;
        var likely = FloorZero(Money.Round(intercept + slope * xFuture));
        var conservative = FloorZero(Money.Round(likely - stdDev));
        var optimistic = FloorZero(Money.Round(likely + stdDev));

        decimal? averageIpca = null;
        decimal? realLikely = null;
        string? criterion = null;
        if (indices is { Count: > 0 })
        {
            averageIpca = AverageMonthlyIpca(indices, lastCompetence);
            if (averageIpca is not null)
            {
                criterion = InflationCriterion;
                var cumulative = 1m;
                for (var month = 0; month < horizonMonths; month++)
                    cumulative *= 1m + averageIpca.Value;
                realLikely = FloorZero(Money.Round(likely / cumulative));
            }
        }

        var historical = sample.Select(record => new ProjectionPoint(
            record.Year,
            record.Month,
            Value(record, basis),
            null,
            null,
            null)).ToList();

        var projected = new List<ProjectionPoint>();
        for (var step = 1; step <= horizonMonths; step++)
        {
            var competence = lastCompetence.AddMonths(step);
            var x = n - 1 + step;
            var mid = FloorZero(Money.Round(intercept + slope * x));
            projected.Add(new ProjectionPoint(
                competence.Year,
                competence.Month,
                null,
                FloorZero(Money.Round(mid - stdDev)),
                mid,
                FloorZero(Money.Round(mid + stdDev))));
        }

        return new ProjectionResult(
            Method,
            sample.Count,
            horizonMonths,
            Disclaimer,
            criterion,
            [
                new ProjectionScenario("conservative", conservative, null),
                new ProjectionScenario("likely", likely, realLikely),
                new ProjectionScenario("optimistic", optimistic, null)
            ],
            historical.Concat(projected).ToList());
    }

    private static decimal Value(SalaryRecord record, SalaryBasis basis)
        => basis == SalaryBasis.Gross ? record.GrossAmount : record.NetAmount;

    private static decimal FloorZero(decimal value) => value < 0m ? 0m : value;

    private static decimal? AverageMonthlyIpca(
        IReadOnlyDictionary<(int Year, int Month), decimal> indices,
        Competence end)
    {
        var keys = indices.Keys
            .Where(key => key.Year * 12 + key.Month <= end.Key)
            .OrderByDescending(key => key.Year)
            .ThenByDescending(key => key.Month)
            .Take(13)
            .OrderBy(key => key.Year)
            .ThenBy(key => key.Month)
            .ToList();

        if (keys.Count < 2)
            return null;

        var ratios = new List<decimal>();
        for (var index = 1; index < keys.Count; index++)
        {
            var previous = indices[keys[index - 1]];
            var current = indices[keys[index]];
            if (previous == 0m)
                continue;
            ratios.Add(current / previous - 1m);
        }

        if (ratios.Count == 0)
            return null;

        return ratios.Average();
    }
}
