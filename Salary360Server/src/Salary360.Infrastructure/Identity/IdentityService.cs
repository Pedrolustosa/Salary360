using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Buffers.Text;
using System.Security.Claims;
using System.Text;
using Salary360.Application.Auth;
using Salary360.Application.Common;
using Salary360.Application.Common.Exceptions;
using Salary360.Application.Interfaces.Identity;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;
using Salary360.Infrastructure.Enums;
using Salary360.Infrastructure.Interfaces;
using Salary360.Infrastructure.Persistence;

namespace Salary360.Infrastructure.Identity;

public sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    AppDbContext dbContext,
    IJwtTokenGenerator tokenGenerator,
    ConfirmationEmailService confirmationEmails,
    ILogger<IdentityService> logger) : IIdentityService
{
    public async Task<UserProfileDto> RegisterAsync(
        string email,
        string password,
        string displayName,
        DateOnly dateOfBirth,
        string? phoneNumber,
        DateTimeOffset termsAcceptedAt,
        DateTimeOffset privacyPolicyAcceptedAt,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email.Trim(),
            Email = email.Trim(),
            DisplayName = displayName.Trim(),
            DateOfBirth = dateOfBirth,
            PhoneNumber = NormalizePhone(phoneNumber),
            Status = UserStatus.Active,
            TermsAcceptedAt = termsAcceptedAt,
            PrivacyPolicyAcceptedAt = privacyPolicyAcceptedAt,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            EmailConfirmed = false
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            logger.LogWarning("User registration rejected with {ErrorCount} identity error(s)", result.Errors.Count());
            throw new IdentityOperationException(
                result.Errors.Select(e => e.Description).ToList(),
                result.Errors.Select(e => e.Code).ToList());
        }

        var roleResult = await userManager.AddToRoleAsync(user, AppRoles.User);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            logger.LogError("Could not assign role {Role} to the new user; registration rolled back", AppRoles.User);
            throw new IdentityOperationException(roleResult.Errors.Select(e => e.Description).ToList());
        }

        logger.LogInformation("User {UserId} registered", user.Id);

        try
        {
            await confirmationEmails.SendAsync(user, ConfirmationEmailKind.AccountCreated, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Failed to send the confirmation e-mail to user {UserId}", user.Id);
        }

        return ToProfile(user, [AppRoles.User], null);
    }

    public async Task<AuthResponse> LoginAsync(
        string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email.Trim());
        if (user is null)
            throw new AuthenticationFailedException();

        if (await userManager.IsLockedOutAsync(user))
        {
            logger.LogWarning("Login attempt for locked out user {UserId}", user.Id);
            throw new AccountLockedException(await RemainingLockoutAsync(user));
        }

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            await userManager.AccessFailedAsync(user);
            logger.LogWarning("Invalid password for user {UserId}", user.Id);

            if (await userManager.IsLockedOutAsync(user))
                throw new AccountLockedException(await RemainingLockoutAsync(user));

            throw new AuthenticationFailedException();
        }

        if (user.Status == UserStatus.Inactive)
        {
            logger.LogInformation("Login blocked for user {UserId}: account inactive", user.Id);
            throw new UserInactiveException();
        }

        if (!user.EmailConfirmed)
        {
            logger.LogInformation("Login blocked for user {UserId}: e-mail not confirmed", user.Id);
            throw new EmailNotConfirmedException();
        }

        await userManager.ResetAccessFailedCountAsync(user);
        var roles = await userManager.GetRolesAsync(user);

        logger.LogInformation("User {UserId} signed in", user.Id);
        var permissions = await GetPermissionsAsync(roles);
        return tokenGenerator.Generate(user, roles, permissions);
    }

    private async Task<TimeSpan> RemainingLockoutAsync(ApplicationUser user)
    {
        var end = await userManager.GetLockoutEndDateAsync(user);
        var remaining = end.HasValue ? end.Value - DateTimeOffset.UtcNow : TimeSpan.Zero;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.FromMinutes(1);
    }

    public async Task ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new IdentityOperationException(["The confirmation link is invalid."], ["InvalidConfirmationLink"]);

        if (user.EmailConfirmed)
            return;

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(Base64Url.DecodeFromChars(token));
        }
        catch (FormatException)
        {
            throw new IdentityOperationException(["The confirmation link is invalid."], ["InvalidConfirmationLink"]);
        }

        var result = await userManager.ConfirmEmailAsync(user, decodedToken);
        if (!result.Succeeded)
        {
            logger.LogWarning("E-mail confirmation failed for user {UserId}", user.Id);
            throw new IdentityOperationException(["The confirmation link is invalid or has expired."], ["InvalidConfirmationLink"]);
        }

        logger.LogInformation("User {UserId} confirmed the e-mail", user.Id);
    }

    public async Task ResendConfirmationAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email.Trim());

        if (user is null || user.EmailConfirmed)
            return;

        await confirmationEmails.SendAsync(user, ConfirmationEmailKind.Resend, cancellationToken);
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
                   ?? throw new NotFoundException(nameof(ApplicationUser), userId);

        return ToProfile(user, await userManager.GetRolesAsync(user), await FindAddressAsync(user.Id, cancellationToken));
    }

    public async Task<UserProfileDto> UpdateProfileAsync(
        Guid userId,
        string displayName,
        DateOnly dateOfBirth,
        string? phoneNumber,
        string? profilePictureUrl,
        UserAddressDto? address,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
                   ?? throw new NotFoundException(nameof(ApplicationUser), userId);

        user.DisplayName = displayName.Trim();
        user.DateOfBirth = dateOfBirth;
        user.PhoneNumber = NormalizePhone(phoneNumber);
        user.ProfilePictureUrl = string.IsNullOrWhiteSpace(profilePictureUrl) ? null : profilePictureUrl.Trim();
        user.UpdatedAtUtc = DateTime.UtcNow;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new IdentityOperationException(
                result.Errors.Select(e => e.Description).ToList(),
                result.Errors.Select(e => e.Code).ToList());

        var savedAddress = await SaveAddressAsync(user.Id, address, cancellationToken);

        logger.LogInformation("User {UserId} updated the profile", user.Id);
        return ToProfile(user, await userManager.GetRolesAsync(user), savedAddress);
    }

    public async Task<UserProfileDto> SetStatusAsync(
        Guid userId, UserStatus status, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
                   ?? throw new NotFoundException(nameof(ApplicationUser), userId);

        user.Status = status;
        user.UpdatedAtUtc = DateTime.UtcNow;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new IdentityOperationException(
                result.Errors.Select(e => e.Description).ToList(),
                result.Errors.Select(e => e.Code).ToList());

        logger.LogInformation("User {UserId} status changed to {Status}", user.Id, status);
        return ToProfile(user, await userManager.GetRolesAsync(user), await FindAddressAsync(user.Id, cancellationToken));
    }

    public async Task<IReadOnlyList<UserProfileDto>> ListUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await userManager.Users
            .OrderBy(user => user.DisplayName)
            .Take(200)
            .ToListAsync(cancellationToken);

        var userIds = users.Select(user => user.Id).ToList();
        var addresses = await dbContext.UserAddresses
            .AsNoTracking()
            .Where(address => userIds.Contains(address.UserId))
            .ToDictionaryAsync(address => address.UserId, cancellationToken);

        var profiles = new List<UserProfileDto>(users.Count);
        foreach (var user in users)
            profiles.Add(ToProfile(user, await userManager.GetRolesAsync(user), addresses.GetValueOrDefault(user.Id)));

        return profiles;
    }

    public async Task<IReadOnlyList<RoleDto>> ListRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await roleManager.Roles
            .OrderBy(role => role.Name)
            .ToListAsync(cancellationToken);

        var result = new List<RoleDto>(roles.Count);
        foreach (var role in roles)
            result.Add(new RoleDto(role.Name!, await GetRolePermissionsAsync(role)));

        return result;
    }

    public async Task<RoleDto> UpdateRolePermissionsAsync(
        string roleName,
        IReadOnlyList<string> permissions,
        CancellationToken cancellationToken = default)
    {
        var role = await roleManager.FindByNameAsync(roleName) ?? throw new NotFoundException("Role", roleName);

        if (string.Equals(role.Name, AppRoles.Admin, StringComparison.OrdinalIgnoreCase))
            throw new IdentityOperationException(["The Admin role cannot be modified."], ["AdminRoleLocked"]);

        var desired = permissions.Distinct().ToList();
        var current = (await roleManager.GetClaimsAsync(role))
            .Where(claim => claim.Type == AppPermissions.ClaimType)
            .ToList();

        foreach (var obsolete in current.Where(claim => !desired.Contains(claim.Value)))
            await EnsureRoleResultAsync(roleManager.RemoveClaimAsync(role, obsolete));

        foreach (var missing in desired.Where(permission => current.All(claim => claim.Value != permission)))
            await EnsureRoleResultAsync(roleManager.AddClaimAsync(role, new Claim(AppPermissions.ClaimType, missing)));

        logger.LogInformation("Permissions of role {Role} updated", role.Name);
        return new RoleDto(role.Name!, await GetRolePermissionsAsync(role));
    }

    private static async Task EnsureRoleResultAsync(Task<IdentityResult> operation)
    {
        var result = await operation;
        if (!result.Succeeded)
            throw new IdentityOperationException(
                result.Errors.Select(error => error.Description).ToList(),
                result.Errors.Select(error => error.Code).ToList());
    }

    private async Task<IReadOnlyList<string>> GetPermissionsAsync(IEnumerable<string> roleNames)
    {
        var permissions = new HashSet<string>();
        foreach (var roleName in roleNames)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
                continue;

            foreach (var permission in await GetRolePermissionsAsync(role))
                permissions.Add(permission);
        }

        return permissions.OrderBy(permission => permission).ToList();
    }

    private async Task<IReadOnlyList<string>> GetRolePermissionsAsync(IdentityRole<Guid> role)
    {
        var claims = await roleManager.GetClaimsAsync(role);
        return claims
            .Where(claim => claim.Type == AppPermissions.ClaimType)
            .Select(claim => claim.Value)
            .OrderBy(value => value)
            .ToList();
    }

    private static string? NormalizePhone(string? phoneNumber)
        => string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();

    private Task<UserAddress?> FindAddressAsync(Guid userId, CancellationToken cancellationToken)
        => dbContext.UserAddresses
            .AsNoTracking()
            .FirstOrDefaultAsync(address => address.UserId == userId, cancellationToken);

    private async Task<UserAddress?> SaveAddressAsync(
        Guid userId, UserAddressDto? dto, CancellationToken cancellationToken)
    {
        var existing = await dbContext.UserAddresses
            .FirstOrDefaultAsync(address => address.UserId == userId, cancellationToken);

        if (dto is null)
        {
            if (existing is not null)
            {
                dbContext.UserAddresses.Remove(existing);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return null;
        }

        if (existing is null)
        {
            existing = new UserAddress(
                userId, dto.PostalCode, dto.Street, dto.Number, dto.Complement,
                dto.Neighborhood, dto.City, dto.State, dto.Country);
            dbContext.UserAddresses.Add(existing);
        }
        else
        {
            existing.Update(
                dto.PostalCode, dto.Street, dto.Number, dto.Complement,
                dto.Neighborhood, dto.City, dto.State, dto.Country);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }

    private static UserProfileDto ToProfile(ApplicationUser user, IEnumerable<string> roles, UserAddress? address) => new(
        user.Id,
        user.Email!,
        user.DisplayName,
        user.DateOfBirth,
        user.PhoneNumber,
        user.Status,
        user.TermsAcceptedAt,
        user.PrivacyPolicyAcceptedAt,
        user.ProfilePictureUrl,
        address is null
            ? null
            : new UserAddressDto(
                address.PostalCode,
                address.Street,
                address.Number,
                address.Complement,
                address.Neighborhood,
                address.City,
                address.State,
                address.Country),
        DateTime.SpecifyKind(user.CreatedAtUtc, DateTimeKind.Utc),
        user.UpdatedAtUtc is null ? null : DateTime.SpecifyKind(user.UpdatedAtUtc.Value, DateTimeKind.Utc),
        roles.OrderBy(role => role).ToList());
}
