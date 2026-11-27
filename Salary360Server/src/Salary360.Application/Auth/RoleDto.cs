namespace Salary360.Application.Auth;

public sealed record RoleDto(string Name, IReadOnlyList<string> Permissions);
