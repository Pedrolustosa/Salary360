namespace Salary360.Application.Salaries.Dtos;

public sealed record PagedSalariesDto(
    IReadOnlyList<SalaryRecordDto> Items,
    int Page,
    int PageSize,
    int TotalCount);
