using Salary360.Application.Expenses.Dtos;

namespace Salary360.API.Contracts.Responses.Expenses;

public static class ExpenseResponseMapping
{
    public static ExpenseResponse ToResponse(this ExpenseDto dto)
        => new(
            dto.Id,
            dto.CategoryId,
            dto.Name,
            dto.Type,
            dto.Amount,
            dto.Year,
            dto.Month,
            dto.DueDay,
            dto.IsPaid,
            dto.PaidAtUtc,
            dto.Notes,
            dto.SeriesId,
            dto.CreatedAtUtc,
            dto.UpdatedAtUtc);

    public static IReadOnlyList<ExpenseResponse> ToResponse(this IReadOnlyList<ExpenseDto> dtos)
        => dtos.Select(x => x.ToResponse()).ToList();

    public static ExpenseListItemResponse ToResponse(this ExpenseListItemDto dto)
        => new(
            dto.Id,
            dto.CategoryId,
            dto.CategoryName,
            dto.CategorySystemKey,
            dto.CategoryColorKey,
            dto.CategoryIconKey,
            dto.Name,
            dto.Type,
            dto.Amount,
            dto.Year,
            dto.Month,
            dto.DueDay,
            dto.IsPaid,
            dto.PaidAtUtc,
            dto.SeriesId);

    public static PagedExpensesResponse ToResponse(this PagedExpensesDto dto)
        => new(
            dto.Items.Select(x => x.ToResponse()).ToList(),
            dto.Total,
            dto.Page,
            dto.PageSize);

    public static ExpenseCategoryResponse ToResponse(this ExpenseCategoryDto dto)
        => new(
            dto.Id,
            dto.Name,
            dto.SystemKey,
            dto.ColorKey,
            dto.IconKey,
            dto.CreatedAtUtc,
            dto.UpdatedAtUtc);

    public static ExpenseCategoryListItemResponse ToResponse(this ExpenseCategoryListItemDto dto)
        => new(
            dto.Id,
            dto.Name,
            dto.SystemKey,
            dto.ColorKey,
            dto.IconKey,
            dto.ExpenseCount,
            dto.CreatedAtUtc,
            dto.UpdatedAtUtc);

    public static IReadOnlyList<ExpenseCategoryListItemResponse> ToResponse(
        this IReadOnlyList<ExpenseCategoryListItemDto> dtos)
        => dtos.Select(x => x.ToResponse()).ToList();

    public static ExpenseCategorySummaryResponse ToResponse(this ExpenseCategorySummaryDto dto)
        => new(
            dto.CategoryId,
            dto.Name,
            dto.SystemKey,
            dto.ColorKey,
            dto.IconKey,
            dto.Total,
            dto.PercentOfTotal);

    public static ExpenseMonthSummaryResponse ToResponse(this ExpenseMonthSummaryDto dto)
        => new(
            dto.TotalFixed,
            dto.TotalVariable,
            dto.Total,
            dto.TotalPaid,
            dto.TotalPending,
            dto.Count,
            dto.ByCategory.Select(x => x.ToResponse()).ToList(),
            dto.HasSalary,
            dto.NetAmount,
            dto.CommittedPercent,
            dto.Available);

    public static ExpenseSeriesPointResponse ToResponse(this ExpenseSeriesPointDto dto)
        => new(dto.Year, dto.Month, dto.TotalFixed, dto.TotalVariable);

    public static IReadOnlyList<ExpenseSeriesPointResponse> ToResponse(this IReadOnlyList<ExpenseSeriesPointDto> dtos)
        => dtos.Select(x => x.ToResponse()).ToList();

    public static CopyExpensesResultResponse ToResponse(this CopyExpensesResultDto dto)
        => new(dto.Created, dto.Skipped);

    public static CreateDefaultsResultResponse ToResponse(this CreateDefaultsResultDto dto)
        => new(dto.Created);
}
