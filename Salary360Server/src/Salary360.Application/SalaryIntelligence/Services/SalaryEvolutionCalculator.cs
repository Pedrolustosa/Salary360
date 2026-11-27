using Salary360.Application.SalaryIntelligence.Interfaces;
using Salary360.Application.SalaryIntelligence.Models;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;

namespace Salary360.Application.SalaryIntelligence.Services;

public sealed class SalaryEvolutionCalculator : ISalaryEvolutionCalculator
{
    public EvolutionResult Calculate(IReadOnlyList<SalaryRecord> records)
    {
        if (records.Count == 0)
            throw new ArgumentException("At least one record is required.", nameof(records));

        var ordered = records
            .OrderBy(record => record.Year)
            .ThenBy(record => record.Month)
            .ToList();

        var first = ordered[0];
        var last = ordered[^1];
        var grossAbs = PercentMath.AbsoluteChange(first.GrossAmount, last.GrossAmount)!.Value;
        var netAbs = PercentMath.AbsoluteChange(first.NetAmount, last.NetAmount)!.Value;

        var series = new List<SeriesPointResult>(ordered.Count);
        SalaryRecord? previous = null;
        foreach (var record in ordered)
        {
            decimal? grossAbsChange = null;
            decimal? grossPct = null;
            decimal? netAbsChange = null;
            decimal? netPct = null;
            if (previous is not null)
            {
                grossAbsChange = PercentMath.AbsoluteChange(previous.GrossAmount, record.GrossAmount);
                grossPct = PercentMath.PercentChange(previous.GrossAmount, record.GrossAmount);
                netAbsChange = PercentMath.AbsoluteChange(previous.NetAmount, record.NetAmount);
                netPct = PercentMath.PercentChange(previous.NetAmount, record.NetAmount);
            }

            series.Add(new SeriesPointResult(
                record.Year,
                record.Month,
                record.GrossAmount,
                record.NetAmount,
                record.TotalBenefits,
                record.TotalDeductions,
                grossAbsChange,
                grossPct,
                netAbsChange,
                netPct));
            previous = record;
        }

        return new EvolutionResult(
            new EvolutionChange(grossAbs, PercentMath.PercentChange(first.GrossAmount, last.GrossAmount)),
            new EvolutionChange(netAbs, PercentMath.PercentChange(first.NetAmount, last.NetAmount)),
            series,
            Composition(first, SalaryItemType.Benefit),
            Composition(first, SalaryItemType.Deduction),
            Composition(last, SalaryItemType.Benefit),
            Composition(last, SalaryItemType.Deduction),
            first.TotalBenefits,
            first.TotalDeductions,
            last.TotalBenefits,
            last.TotalDeductions);
    }

    private static IReadOnlyList<CompositionItemResult> Composition(SalaryRecord record, SalaryItemType type)
        => record.Items
            .Where(item => item.Type == type)
            .OrderBy(item => item.Order)
            .Select(item => new CompositionItemResult(
                item.Name,
                item.Amount,
                PercentMath.Ratio(item.Amount, record.GrossAmount)))
            .ToList();
}
