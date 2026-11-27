using Salary360.Application.Auth;
using Salary360.Infrastructure.Identity;

namespace Salary360.Infrastructure.Interfaces;

public interface IJwtTokenGenerator
{
    AuthResponse Generate(ApplicationUser user, IEnumerable<string> roles, IEnumerable<string> permissions);
}
