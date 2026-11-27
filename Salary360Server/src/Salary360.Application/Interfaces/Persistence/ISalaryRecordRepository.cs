using Salary360.Application.Salaries;
using Salary360.Domain.Entities;

namespace Salary360.Application.Interfaces.Persistence;

public interface ISalaryRecordRepository
{
    Task<SalaryRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<SalaryRecord?> GetByCompetenceAsync(
        Guid userId, int year, int month, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid userId, int year, int month, Guid? excludingId = null, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<SalaryRecord> Items, int Total)> ListAsync(
        Guid userId, int? year, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SalaryEvolutionPointDto>> GetMonthEvolutionAsync(
        Guid userId, int month, int fromYear, int toYear, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SalaryEvolutionPointDto>> GetYearEvolutionAsync(
        Guid userId, int fromYear, int toYear, CancellationToken cancellationToken = default);

    void Add(SalaryRecord record);

    void Remove(SalaryRecord record);
}
