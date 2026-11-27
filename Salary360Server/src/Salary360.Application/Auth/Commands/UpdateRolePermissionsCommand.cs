using FluentValidation;
using Salary360.Application.Common;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;

namespace Salary360.Application.Auth.Commands;

public sealed record UpdateRolePermissionsCommand(string RoleName, IReadOnlyList<string> Permissions)
    : ICommand<RoleDto>;

public sealed class UpdateRolePermissionsCommandValidator : AbstractValidator<UpdateRolePermissionsCommand>
{
    public UpdateRolePermissionsCommandValidator()
    {
        RuleFor(x => x.RoleName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Permissions).NotNull().Must(list => list is null || list.Count <= 100);
        RuleForEach(x => x.Permissions).Must(permission => AppPermissions.All.Contains(permission));
    }
}

public sealed class UpdateRolePermissionsCommandHandler(IIdentityService identityService)
    : ICommandHandler<UpdateRolePermissionsCommand, RoleDto>
{
    public Task<RoleDto> HandleAsync(UpdateRolePermissionsCommand request, CancellationToken cancellationToken = default)
        => identityService.UpdateRolePermissionsAsync(request.RoleName, request.Permissions, cancellationToken);
}
