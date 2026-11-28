using Salary360.Application.Common.Interfaces;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Application.SalaryIntelligence.Interfaces;
using Salary360.Application.SalaryIntelligence.Dtos;

namespace Salary360.Application.SalaryIntelligence.Queries;

public sealed class GetSalaryProjectionQueryHandler(
    ISalaryRecordRepository repository,
    ICurrentUser currentUser,
    ISalaryProjectionCalculator projectionCalculator,
    IInflationProvider inflationProvider)
    : IQueryHandler<GetSalaryProjectionQuery, SalaryProjectionDto>
{
    public async Task<SalaryProjectionDto> HandleAsync(
        GetSalaryProjectionQuery request, CancellationToken cancellationToken = default)
    {
        var loaded = await SalaryIntelligenceQuerySupport.LoadAsync(
            currentUser.GetUserId(), request.From, request.To, repository, cancellationToken);

        if (loaded is null)
            return Insufficient(0, null, null);

        var (from, to, records) = loaded.Value;
        if (records.Count < SalaryAnalysisPeriodResolver.ProjectionRequiredRecords)
            return Insufficient(records.Count, from, to);

        var inflation = await inflationProvider.GetIndicesAsync(cancellationToken);
        var projection = projectionCalculator.Calculate(
            records,
            request.Basis,
            request.HorizonMonths,
            inflation.Available ? inflation.Indices : null);

        if (projection is null)
            return Insufficient(records.Count, from, to);

        return new SalaryProjectionDto(
            true,
            SalaryAnalysisPeriodResolver.ProjectionRequiredRecords,
            records.Count,
            SalaryIntelligenceQuerySupport.ToDto(from),
            SalaryIntelligenceQuerySupport.ToDto(to),
            projection.Method,
            projection.SampleSize,
            projection.HorizonMonths,
            projection.DisclaimerCode,
            projection.InflationCriterion,
            projection.Scenarios
                .Select(scenario => new ProjectionScenarioDto(scenario.Name, scenario.Value, scenario.RealValue))
                .ToList(),
            projection.Series
                .Select(point => new ProjectionPointDto(
                    point.Year,
                    point.Month,
                    point.Actual,
                    point.Conservative,
                    point.Likely,
                    point.Optimistic))
                .ToList());
}

    private static SalaryProjectionDto Insufficient(int count, Competence? from, Competence? to)
        => new(
            false,
            SalaryAnalysisPeriodResolver.ProjectionRequiredRecords,
            count,
            from is null ? null : SalaryIntelligenceQuerySupport.ToDto(from.Value),
            to is null ? null : SalaryIntelligenceQuerySupport.ToDto(to.Value),
            null,
            null,
            null,
            null,
            null,
            [],
            []);
}