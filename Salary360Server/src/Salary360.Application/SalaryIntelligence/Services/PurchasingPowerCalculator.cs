using Salary360.Application.SalaryIntelligence.Enums;
using Salary360.Application.SalaryIntelligence.Interfaces;
using Salary360.Application.SalaryIntelligence.Models;
using Salary360.Domain.Common;
using Salary360.Domain.Entities;

namespace Salary360.Application.SalaryIntelligence.Services;

public sealed class PurchasingPowerCalculator : IPurchasingPowerCalculator
{
    public PurchasingPowerResult Calculate(
        IReadOnlyList<SalaryRecord> records,
        SalaryBasis basis,
        IReadOnlyDictionary<(int Year, int Month), decimal> indices,
        bool isStale)
    {
        var ordered = records
            .OrderBy(record => record.Year)
            .ThenBy(record => record.Month)
            .ToList();

        var first = ordered[0];
        var last = ordered[^1];
        var firstValue = Value(first, basis);
        var lastValue = Value(last, basis);
        var nominalRatio = firstValue == 0m ? 0m : (lastValue - firstValue) / firstValue;
        var nominalPercent = PercentMath.RoundPercent(nominalRatio * 100m);

        if (!indices.TryGetValue((first.Year, first.Month), out var startIndex) || startIndex == 0m)
        {
            return new PurchasingPowerResult(
                false,
                isStale,
                null,
                nominalPercent,
                null,
                null,
                null,
                BuildSeriesWithoutInflation(ordered, basis));
        }

        var endYear = last.Year;
        var endMonth = last.Month;
        var isPartial = false;
        if (!indices.TryGetValue((endYear, endMonth), out var endIndex))
        {
            var latest = indices.Keys
                .Where(key => key.Year * 12 + key.Month <= endYear * 12 + endMonth)
                .OrderByDescending(key => key.Year)
                .ThenByDescending(key => key.Month)
                .Cast<(int Year, int Month)?>()
                .FirstOrDefault();

            if (latest is null || !indices.TryGetValue(latest.Value, out endIndex) || endIndex == 0m)
            {
                return new PurchasingPowerResult(
                    false,
                    isStale,
                    null,
                    nominalPercent,
                    null,
                    null,
                    null,
                    BuildSeriesWithoutInflation(ordered, basis));
            }

            endYear = latest.Value.Year;
            endMonth = latest.Value.Month;
            isPartial = true;
        }

        var ipcaAccumulated = endIndex / startIndex - 1m;
        var realRatio = (1m + nominalRatio) / (1m + ipcaAccumulated) - 1m;
        var initialAdjusted = Money.Round(firstValue * (1m + ipcaAccumulated));
        var realAbsolute = Money.Round(lastValue - initialAdjusted);

        var series = ordered.Select(record =>
        {
            var actual = Value(record, basis);
            decimal? adjusted = null;
            if (indices.TryGetValue((record.Year, record.Month), out var index) && startIndex != 0m)
                adjusted = Money.Round(firstValue * (index / startIndex));
            return new PurchasingPowerPoint(record.Year, record.Month, actual, adjusted);
        }).ToList();

        return new PurchasingPowerResult(
            true,
            isStale,
            new InflationCoverage(first.Year, first.Month, endYear, endMonth, isPartial),
            nominalPercent,
            PercentMath.RoundPercent(ipcaAccumulated * 100m),
            PercentMath.RoundPercent(realRatio * 100m),
            realAbsolute,
            series);
    }

    private static decimal Value(SalaryRecord record, SalaryBasis basis)
        => basis == SalaryBasis.Gross ? record.GrossAmount : record.NetAmount;

    private static IReadOnlyList<PurchasingPowerPoint> BuildSeriesWithoutInflation(
        IReadOnlyList<SalaryRecord> ordered, SalaryBasis basis)
        => ordered
            .Select(record => new PurchasingPowerPoint(
                record.Year,
                record.Month,
                Value(record, basis),
                null))
            .ToList();
}
