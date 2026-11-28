namespace Salary360.Application.Auth.Dtos;

public sealed record RoleDto(string Name, IReadOnlyList<string> Permissions);
