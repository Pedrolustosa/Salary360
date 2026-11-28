using FluentValidation;
using Salary360.Application.Common;

namespace Salary360.Application.Auth.Commands;

public sealed class UpdateRolePermissionsCommandValidator : AbstractValidator<UpdateRolePermissionsCommand>
{
    public UpdateRolePermissionsCommandValidator()
    {
        RuleFor(x => x.RoleName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Permissions).NotNull().Must(list => list is null || list.Count <= 100);
        RuleForEach(x => x.Permissions).Must(permission => AppPermissions.All.Contains(permission));
    }
}