using Salary360.Application.Common;
using Salary360.Application.Interfaces.Messaging;

namespace Salary360.Application.Salaries.Commands;

public sealed record DeleteSalaryCommand(Guid Id) : ICommand<Unit>;
