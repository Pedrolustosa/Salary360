using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Auth.Dtos;

namespace Salary360.Application.Auth.Commands;

public sealed record LoginCommand(string Email, string Password) : ICommand<AuthResponse>;
