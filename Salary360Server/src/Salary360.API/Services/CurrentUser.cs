using System.Security.Claims;
using Salary360.Application.Common.Exceptions;
using Salary360.Application.Interfaces.Identity;

namespace Salary360.API.Services;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid GetUserId()
    {
        var subject = httpContextAccessor.HttpContext?.User.FindFirstValue("sub");
        return Guid.TryParse(subject, out var userId)
            ? userId
            : throw new AuthenticationFailedException("Authentication is required.");
    }
}