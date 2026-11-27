namespace Salary360.Application.Salaries;

public sealed record PagedSalariesDto(
    IReadOnlyList<SalaryRecordDto> Items,
    int Page,
    int PageSize,
    int TotalCount);
