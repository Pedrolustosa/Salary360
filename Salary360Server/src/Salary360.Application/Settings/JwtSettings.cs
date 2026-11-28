using System.ComponentModel.DataAnnotations;

namespace Salary360.Application.Settings;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    private const string SecretHelp =
        "Jwt:Secret is missing or shorter than 32 characters. Run scripts/setup-secrets.sh (or setup-secrets.ps1) " +
        "or: dotnet user-secrets set \"Jwt:Secret\" \"<random 32+ characters>\" --project src/Salary360.API";

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Required(ErrorMessage = SecretHelp), MinLength(32, ErrorMessage = SecretHelp)]
    public string Secret { get; init; } = string.Empty;

    [Range(1, 1440)]
    public int ExpirationMinutes { get; init; } = 60;
}