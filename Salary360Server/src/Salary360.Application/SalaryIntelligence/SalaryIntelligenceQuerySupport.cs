using Salary360.Application.Interfaces.Persistence;
using Salary360.Domain.Entities;
using Salary360.Application.SalaryIntelligence.Dtos;

namespace Salary360.Application.SalaryIntelligence;

public static class SalaryIntelligenceQuerySupport
{
    public static async Task<(Competence From, Competence To, IReadOnlyList<SalaryRecord> Records)?> LoadAsync(
        Guid userId,
        string? from,
        string? to,
        ISalaryRecordRepository repository,
        CancellationToken cancellationToken)
    {
        var period = await SalaryAnalysisPeriodResolver.ResolveAsync(userId, from, to, repository, cancellationToken);
        if (period is null)
            return null;

        var (start, end) = period.Value;
        var records = await repository.ListByPeriodAsync(
            userId, start.Year, start.Month, end.Year, end.Month, cancellationToken);
        return (start, end, records);
    }

    public static CompetenceDto ToDto(Competence competence)
        => new(competence.Year, competence.Month);
}