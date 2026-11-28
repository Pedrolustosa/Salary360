using Salary360.Application.SalaryIntelligence.Enums;
using Salary360.Application.SalaryIntelligence.Models;
using Salary360.Domain.Entities;

namespace Salary360.Application.SalaryIntelligence.Interfaces;

public interface ISalaryProjectionCalculator
{
    ProjectionResult? Calculate(
        IReadOnlyList<SalaryRecord> records,
        SalaryBasis basis,
        int horizonMonths,
        IReadOnlyDictionary<(int Year, int Month), decimal>? indices);
}