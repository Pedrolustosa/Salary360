using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Auth.Dtos;

namespace Salary360.Application.Auth.Queries;

public sealed record GetCurrentUserQuery : IQuery<UserProfileDto>;
