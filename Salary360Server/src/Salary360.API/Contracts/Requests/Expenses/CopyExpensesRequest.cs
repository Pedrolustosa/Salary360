namespace Salary360.API.Contracts.Requests.Expenses;

public sealed record CopyExpensesRequest(
    int FromYear,
    int FromMonth,
    int ToYear,
    int ToMonth,
    bool OnlyFixed);
