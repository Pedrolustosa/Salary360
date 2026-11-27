using Salary360.Application.Interfaces.Messaging;

namespace Salary360.Application.Salaries.Queries;

public sealed record GetSalaryQuery(Guid Id) : IQuery<SalaryRecordDto>;
