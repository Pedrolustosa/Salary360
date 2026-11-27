using Salary360.Application.Interfaces.Messaging;

namespace Salary360.Application.Salaries.Queries;

public sealed record ListSalariesQuery(int? Year, int Page = 1, int PageSize = 20) : IQuery<PagedSalariesDto>;
