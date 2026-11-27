using System.ComponentModel.DataAnnotations;

namespace Salary360.Infrastructure.Email;

public sealed class ClientSettings
{
    public const string SectionName = "Client";

    [Required, Url]
    public string BaseUrl { get; set; } = string.Empty;
}
