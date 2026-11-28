using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Salary360.Application.Auth;
using Salary360.Application.Common;
using Salary360.Application.Settings;
using Salary360.Infrastructure.Interfaces;
using Salary360.Application.Auth.Dtos;

namespace Salary360.Infrastructure.Identity;

public sealed class JwtTokenGenerator(IOptions<JwtSettings> options, TimeProvider timeProvider) : IJwtTokenGenerator
{
    public AuthResponse Generate(ApplicationUser user, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        var settings = options.Value;
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var expiresAt = now.AddMinutes(settings.ExpirationMinutes);

        var roleList = roles.ToList();
        var permissionList = permissions.Distinct().OrderBy(permission => permission).ToList();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("name", user.DisplayName)
        };
        claims.AddRange(roleList.Select(role => new Claim("role", role)));
        claims.AddRange(permissionList.Select(permission => new Claim(AppPermissions.ClaimType, permission)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret));
        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new AuthResponse(
            user.Id,
            user.Email!,
            user.DisplayName,
            user.DateOfBirth,
            user.ProfilePictureUrl,
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt,
            roleList,
            permissionList);
}
}