using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Salary360.Application.Common;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;
using Salary360.Infrastructure.Identity;
using Salary360.Infrastructure.Persistence;

namespace Salary360.Infrastructure.Seed;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var services = scope.ServiceProvider;

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = services.GetRequiredService<AppDbContext>();
        var settings = services.GetRequiredService<IOptions<SeedSettings>>().Value;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(IdentitySeeder));

        foreach (var (roleName, permissions) in AppPermissions.ByRole)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                role = new IdentityRole<Guid>(roleName) { Id = Guid.NewGuid() };
                EnsureSucceeded(await roleManager.CreateAsync(role), $"create role '{roleName}'");
                await SyncPermissionsAsync(roleManager, role, permissions);
            }
        }

        await SeedUserAsync(userManager, dbContext, settings.Admin, AppRoles.Admin, logger, cancellationToken);
        await SeedUserAsync(userManager, dbContext, settings.User, AppRoles.User, logger, cancellationToken);
    }

    private static async Task SyncPermissionsAsync(
        RoleManager<IdentityRole<Guid>> roleManager, IdentityRole<Guid> role, IReadOnlyList<string> permissions)
    {
        var current = (await roleManager.GetClaimsAsync(role))
            .Where(claim => claim.Type == AppPermissions.ClaimType)
            .ToList();

        foreach (var obsolete in current.Where(claim => !permissions.Contains(claim.Value)))
            EnsureSucceeded(await roleManager.RemoveClaimAsync(role, obsolete), $"remove permission '{obsolete.Value}'");

        foreach (var missing in permissions.Where(permission => current.All(claim => claim.Value != permission)))
            EnsureSucceeded(
                await roleManager.AddClaimAsync(role, new Claim(AppPermissions.ClaimType, missing)),
                $"add permission '{missing}'");
    }

    private static async Task SeedUserAsync(
        UserManager<ApplicationUser> userManager,
        AppDbContext dbContext,
        SeedUserSettings settings,
        string role,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Email) || string.IsNullOrWhiteSpace(settings.Password))
        {
            logger.LogWarning("Seed for role {Role} skipped: e-mail or password not configured. Set Seed:{Role}:Password with dotnet user-secrets or an environment variable", role, role);
            return;
        }

        var user = await userManager.FindByEmailAsync(settings.Email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = settings.Email,
                Email = settings.Email,
                DisplayName = string.IsNullOrWhiteSpace(settings.DisplayName) ? role : settings.DisplayName,
                DateOfBirth = DateOnly.TryParse(settings.DateOfBirth, out var dateOfBirth) ? dateOfBirth : null,
                PhoneNumber = string.IsNullOrWhiteSpace(settings.PhoneNumber) ? null : settings.PhoneNumber,
                ProfilePictureUrl = string.IsNullOrWhiteSpace(settings.ProfilePictureUrl) ? null : settings.ProfilePictureUrl,
                Status = UserStatus.Active,
                TermsAcceptedAt = DateTimeOffset.UtcNow,
                PrivacyPolicyAcceptedAt = DateTimeOffset.UtcNow,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                EmailConfirmed = true
            };

            EnsureSucceeded(await userManager.CreateAsync(user, settings.Password), $"create the {role} user");

            await SeedAddressAsync(dbContext, user.Id, settings.Address, cancellationToken);
            logger.LogInformation("Seeded {Role} user {UserId}", role, user.Id);
        }

        if (!await userManager.IsInRoleAsync(user, role))
            EnsureSucceeded(await userManager.AddToRoleAsync(user, role), $"assign role '{role}'");
    }

    private static async Task SeedAddressAsync(
        AppDbContext dbContext, Guid userId, SeedAddressSettings address, CancellationToken cancellationToken)
    {
        if (!address.IsComplete)
            return;

        dbContext.UserAddresses.Add(new UserAddress(
            userId,
            address.PostalCode,
            address.Street,
            address.Number,
            address.Complement,
            address.Neighborhood,
            address.City,
            address.State,
            address.Country));

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void EnsureSucceeded(IdentityResult result, string action)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException(
                $"Seed failed to {action}: {string.Join("; ", result.Errors.Select(e => e.Description))}");
    }
}
