using Salary360.Application.Salaries.Dtos;

namespace Salary360.API.Contracts.Responses.Salaries;

public static class SalaryResponseMapping
{
    public static SalaryItemResponse ToResponse(this SalaryItemDto dto)
        => new(dto.Id, dto.Name, dto.Type, dto.Amount, dto.Order);

    public static SalaryRecordResponse ToResponse(this SalaryRecordDto dto)
        => new(
            dto.Id,
            dto.Year,
            dto.Month,
            dto.GrossAmount,
            dto.TotalBenefits,
            dto.TotalDeductions,
            dto.NetAmount,
            dto.Notes,
            dto.Items.Select(x => x.ToResponse()).ToList(),
            dto.CreatedAtUtc,
            dto.UpdatedAtUtc);

    public static PagedSalariesResponse ToResponse(this PagedSalariesDto dto)
        => new(
            dto.Items.Select(x => x.ToResponse()).ToList(),
            dto.Page,
            dto.PageSize,
            dto.TotalCount);

    public static SalaryEvolutionPointResponse ToResponse(this SalaryEvolutionPointDto dto)
        => new(
            dto.Year,
            dto.Gross,
            dto.Benefits,
            dto.Deductions,
            dto.Net,
            dto.AverageNet,
            dto.MonthsCount);

    public static IReadOnlyList<SalaryEvolutionPointResponse> ToResponse(
        this IReadOnlyList<SalaryEvolutionPointDto> dtos)
        => dtos.Select(x => x.ToResponse()).ToList();
}
