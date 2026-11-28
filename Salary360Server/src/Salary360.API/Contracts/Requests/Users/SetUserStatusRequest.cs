using Salary360.Domain.Enums;

namespace Salary360.API.Contracts.Requests.Users;

public sealed record SetUserStatusRequest(UserStatus Status);
