namespace Salary360.API.Controllers;

public sealed record UpdateRolePermissionsRequest(IReadOnlyList<string> Permissions);
