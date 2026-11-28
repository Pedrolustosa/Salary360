using Salary360.Application.Common;
using Salary360.Application.Interfaces.Messaging;

namespace Salary360.Application.Auth.Commands;

public sealed record ResendConfirmationCommand(string Email) : ICommand<Unit>;
