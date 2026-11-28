using Salary360.Application.SalaryIntelligence.Models;
using Salary360.Domain.Entities;

namespace Salary360.Application.SalaryIntelligence.Interfaces;

public interface ISalaryEvolutionCalculator
{
    EvolutionResult Calculate(IReadOnlyList<SalaryRecord> records);
}