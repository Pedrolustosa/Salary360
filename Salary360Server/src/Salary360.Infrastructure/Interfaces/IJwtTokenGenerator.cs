using Salary360.Application.Auth;
using Salary360.Infrastructure.Identity;
using Salary360.Application.Auth.Dtos;

namespace Salary360.Infrastructure.Interfaces;

public interface IJwtTokenGenerator
{
    AuthResponse Generate(ApplicationUser user, IEnumerable<string> roles, IEnumerable<string> permissions);
}