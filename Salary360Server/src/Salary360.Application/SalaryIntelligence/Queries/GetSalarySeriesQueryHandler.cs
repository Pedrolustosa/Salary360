using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Application.SalaryIntelligence.Dtos;
using Salary360.Application.SalaryIntelligence.Interfaces;

namespace Salary360.Application.SalaryIntelligence.Queries;

public sealed class GetSalarySeriesQueryHandler(
    ISalaryRecordRepository repository,
    ICurrentUser currentUser,
    ISalaryEvolutionCalculator evolutionCalculator)
    : IQueryHandler<GetSalarySeriesQuery, SalarySeriesDto>
{
    public async Task<SalarySeriesDto> HandleAsync(
        GetSalarySeriesQuery request, CancellationToken cancellationToken = default)
    {
        var loaded = await SalaryIntelligenceQuerySupport.LoadAsync(
            currentUser.UserId, request.From, request.To, repository, cancellationToken);

        if (loaded is null)
        {
            return new SalarySeriesDto(
                false,
                SalaryAnalysisPeriodResolver.EvolutionRequiredRecords,
                0,
                null,
                null,
                []);
        }

        var (from, to, records) = loaded.Value;
        if (records.Count < SalaryAnalysisPeriodResolver.EvolutionRequiredRecords)
        {
            return new SalarySeriesDto(
                false,
                SalaryAnalysisPeriodResolver.EvolutionRequiredRecords,
                records.Count,
                SalaryIntelligenceQuerySupport.ToDto(from),
                SalaryIntelligenceQuerySupport.ToDto(to),
                []);
        }

        var evolution = evolutionCalculator.Calculate(records);
        var points = evolution.Series
            .Select(point => new SalarySeriesPointDto(
                point.Year,
                point.Month,
                point.Gross,
                point.Net,
                point.Benefits,
                point.Deductions,
                point.GrossChangeAbsolute,
                point.GrossChangePercent,
                point.NetChangeAbsolute,
                point.NetChangePercent))
            .ToList();

        return new SalarySeriesDto(
            true,
            SalaryAnalysisPeriodResolver.EvolutionRequiredRecords,
            records.Count,
            SalaryIntelligenceQuerySupport.ToDto(from),
            SalaryIntelligenceQuerySupport.ToDto(to),
            points);
    }
}
