using System.ComponentModel.DataAnnotations;

namespace Salary360.Infrastructure.Email;

public sealed class EmailSettings
{
    public const string SectionName = "Email";

    public string Provider { get; set; } = "Smtp";

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string FromAddress { get; set; } = string.Empty;

    public string FromName { get; set; } = string.Empty;

    [Range(0, 86400)]
    public int ResendIntervalSeconds { get; set; } = 60;
}