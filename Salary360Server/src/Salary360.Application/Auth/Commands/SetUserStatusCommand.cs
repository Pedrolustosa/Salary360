using Salary360.Application.Interfaces.Messaging;
using Salary360.Domain.Enums;
using Salary360.Application.Auth.Dtos;

namespace Salary360.Application.Auth.Commands;

public sealed record SetUserStatusCommand(Guid UserId, UserStatus Status) : ICommand<UserProfileDto>;
