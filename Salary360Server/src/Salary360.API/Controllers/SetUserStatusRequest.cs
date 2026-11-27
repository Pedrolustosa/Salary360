using Salary360.Domain.Enums;

namespace Salary360.API.Controllers;

public sealed record SetUserStatusRequest(UserStatus Status);
