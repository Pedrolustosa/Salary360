namespace Salary360.API.Contracts.Responses.Auth;

public sealed record RoleResponse(string Name, IReadOnlyList<string> Permissions);
