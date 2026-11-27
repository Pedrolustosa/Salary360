using System.ComponentModel.DataAnnotations;

namespace Salary360.Application.Settings;

public sealed class IbgeSettings
{
    public const string SectionName = "Ibge";

    [Required, Url]
    public string BaseUrl { get; init; } = "https://apisidra.ibge.gov.br";

    [Required]
    public string Table { get; init; } = "1737";

    [Required]
    public string Variable { get; init; } = "2266";

    [Range(1, 168)]
    public int CacheHours { get; init; } = 12;

    [Range(1, 720)]
    public int StaleCacheHours { get; init; } = 168;

    [Range(1, 60)]
    public int TimeoutSeconds { get; init; } = 10;
}
