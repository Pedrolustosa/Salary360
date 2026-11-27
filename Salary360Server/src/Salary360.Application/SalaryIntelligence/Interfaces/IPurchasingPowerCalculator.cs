using Salary360.Application.SalaryIntelligence.Enums;
using Salary360.Application.SalaryIntelligence.Models;
using Salary360.Domain.Entities;

namespace Salary360.Application.SalaryIntelligence.Interfaces;

public interface IPurchasingPowerCalculator
{
    PurchasingPowerResult Calculate(
        IReadOnlyList<SalaryRecord> records,
        SalaryBasis basis,
        IReadOnlyDictionary<(int Year, int Month), decimal> indices,
        bool isStale);
}
