namespace Salary360.API.Contracts.Requests.Roles;

public sealed record UpdateRolePermissionsRequest(IReadOnlyList<string> Permissions);
