using Salary360.Application.SalaryIntelligence.Dtos;

namespace Salary360.API.Contracts.Responses.SalaryIntelligence;

public static class SalaryIntelligenceResponseMapping
{
    public static CompetenceResponse ToResponse(this CompetenceDto dto)
        => new(dto.Year, dto.Month);

    public static SalaryIntelligenceSummaryResponse ToResponse(this SalaryIntelligenceSummaryDto dto)
        => new(
            dto.HasEnoughData,
            dto.RequiredRecords,
            dto.RecordCount,
            dto.From?.ToResponse(),
            dto.To?.ToResponse(),
            dto.CurrentGross,
            dto.CurrentNet,
            dto.TotalBenefits,
            dto.TotalDeductions,
            dto.GrossChangeAbsolute,
            dto.GrossChangePercent,
            dto.NetChangeAbsolute,
            dto.NetChangePercent);

    public static SalarySeriesPointResponse ToResponse(this SalarySeriesPointDto dto)
        => new(
            dto.Year,
            dto.Month,
            dto.Gross,
            dto.Net,
            dto.Benefits,
            dto.Deductions,
            dto.GrossChangeAbsolute,
            dto.GrossChangePercent,
            dto.NetChangeAbsolute,
            dto.NetChangePercent);

    public static SalarySeriesResponse ToResponse(this SalarySeriesDto dto)
        => new(
            dto.HasEnoughData,
            dto.RequiredRecords,
            dto.RecordCount,
            dto.From?.ToResponse(),
            dto.To?.ToResponse(),
            dto.Points.Select(x => x.ToResponse()).ToList());

    public static InflationCoverageResponse ToResponse(this InflationCoverageDto dto)
        => new(dto.FromYear, dto.FromMonth, dto.ToYear, dto.ToMonth, dto.IsPartial);

    public static PurchasingPowerPointResponse ToResponse(this PurchasingPowerPointDto dto)
        => new(dto.Year, dto.Month, dto.Actual, dto.InflationAdjusted);

    public static PurchasingPowerResponse ToResponse(this PurchasingPowerDto dto)
        => new(
            dto.HasEnoughData,
            dto.RequiredRecords,
            dto.RecordCount,
            dto.From?.ToResponse(),
            dto.To?.ToResponse(),
            dto.InflationAvailable,
            dto.IsStale,
            dto.InflationCoverage?.ToResponse(),
            dto.NominalChangePercent,
            dto.IpcaAccumulatedPercent,
            dto.RealGainPercent,
            dto.RealGainAbsolute,
            dto.Series.Select(x => x.ToResponse()).ToList());

    public static SalaryInsightResponse ToResponse(this SalaryInsightDto dto)
        => new(dto.Code, dto.Severity, dto.Params, dto.ReferenceCompetence);

    public static SalaryInsightsResponse ToResponse(this SalaryInsightsDto dto)
        => new(
            dto.HasEnoughData,
            dto.RequiredRecords,
            dto.RecordCount,
            dto.From?.ToResponse(),
            dto.To?.ToResponse(),
            dto.Insights.Select(x => x.ToResponse()).ToList());

    public static ProjectionScenarioResponse ToResponse(this ProjectionScenarioDto dto)
        => new(dto.Name, dto.Value, dto.RealValue);

    public static ProjectionPointResponse ToResponse(this ProjectionPointDto dto)
        => new(dto.Year, dto.Month, dto.Actual, dto.Conservative, dto.Likely, dto.Optimistic);

    public static SalaryProjectionResponse ToResponse(this SalaryProjectionDto dto)
        => new(
            dto.HasEnoughData,
            dto.RequiredRecords,
            dto.RecordCount,
            dto.From?.ToResponse(),
            dto.To?.ToResponse(),
            dto.Method,
            dto.SampleSize,
            dto.HorizonMonths,
            dto.DisclaimerCode,
            dto.InflationCriterion,
            dto.Scenarios.Select(x => x.ToResponse()).ToList(),
            dto.Series.Select(x => x.ToResponse()).ToList());
}
