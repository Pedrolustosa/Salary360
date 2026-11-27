namespace Salary360.Application.SalaryIntelligence.Models;

public sealed record EvolutionChange(
    decimal Absolute,
    decimal? Percent);

public sealed record SeriesPointResult(
    int Year,
    int Month,
    decimal Gross,
    decimal Net,
    decimal Benefits,
    decimal Deductions,
    decimal? GrossChangeAbsolute,
    decimal? GrossChangePercent,
    decimal? NetChangeAbsolute,
    decimal? NetChangePercent);

public sealed record CompositionItemResult(
    string Name,
    decimal Amount,
    decimal? ShareOfGrossPercent);

public sealed record EvolutionResult(
    EvolutionChange Gross,
    EvolutionChange Net,
    IReadOnlyList<SeriesPointResult> Series,
    IReadOnlyList<CompositionItemResult> FirstBenefits,
    IReadOnlyList<CompositionItemResult> FirstDeductions,
    IReadOnlyList<CompositionItemResult> LastBenefits,
    IReadOnlyList<CompositionItemResult> LastDeductions,
    decimal FirstTotalBenefits,
    decimal FirstTotalDeductions,
    decimal LastTotalBenefits,
    decimal LastTotalDeductions);
