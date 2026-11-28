namespace Salary360.API.Contracts.Responses.Salaries;

public sealed record PagedSalariesResponse(
    IReadOnlyList<SalaryRecordResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
