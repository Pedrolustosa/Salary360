namespace Salary360.Application.Common;

public sealed class ProjectSettings
{
    public const string SectionName = "Project";

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;
}