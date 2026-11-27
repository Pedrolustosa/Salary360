using Salary360.Application.SalaryIntelligence.Enums;
using Salary360.Application.SalaryIntelligence.Models;
using Salary360.Domain.Entities;

namespace Salary360.Application.SalaryIntelligence.Interfaces;

public interface ISalaryInsightGenerator
{
    IReadOnlyList<InsightResult> Generate(
        IReadOnlyList<SalaryRecord> records,
        PurchasingPowerResult? purchasingPower,
        SalaryBasis basis);
}
