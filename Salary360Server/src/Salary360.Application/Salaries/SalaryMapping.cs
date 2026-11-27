using Salary360.Domain.Entities;

namespace Salary360.Application.Salaries;

public static class SalaryMapping
{
    public static SalaryRecordDto ToDto(this SalaryRecord record)
        => new(
            record.Id,
            record.Year,
            record.Month,
            record.GrossAmount,
            record.TotalBenefits,
            record.TotalDeductions,
            record.NetAmount,
            record.Notes,
            record.Items
                .OrderBy(item => item.Order)
                .Select(item => new SalaryItemDto(item.Id, item.Name, item.Type, item.Amount, item.Order))
                .ToList(),
            record.CreatedAtUtc,
            record.UpdatedAtUtc);
}
