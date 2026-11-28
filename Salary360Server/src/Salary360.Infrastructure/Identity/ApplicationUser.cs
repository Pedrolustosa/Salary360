using Microsoft.AspNetCore.Identity;
using Salary360.Domain.Enums;

namespace Salary360.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;

    public DateOnly? DateOfBirth { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;

    public DateTimeOffset? TermsAcceptedAt { get; set; }

    public DateTimeOffset? PrivacyPolicyAcceptedAt { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }
}