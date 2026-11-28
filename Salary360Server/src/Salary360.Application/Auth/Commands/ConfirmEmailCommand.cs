using Salary360.Application.Common;
using Salary360.Application.Interfaces.Messaging;

namespace Salary360.Application.Auth.Commands;

public sealed record ConfirmEmailCommand(Guid UserId, string Token) : ICommand<Unit>;
