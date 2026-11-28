using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Salaries.Dtos;

namespace Salary360.Application.Salaries.Queries;

public sealed record GetSalaryQuery(Guid Id) : IQuery<SalaryRecordDto>;
