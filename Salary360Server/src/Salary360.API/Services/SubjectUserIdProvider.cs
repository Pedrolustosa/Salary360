using Microsoft.AspNetCore.SignalR;

namespace Salary360.API.Services;

public sealed class SubjectUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) => connection.User.FindFirst("sub")?.Value;
}
