using Salary360.Application.Expenses.Interfaces;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Auth.Dtos;

namespace Salary360.Application.Auth.Commands;

public sealed class RegisterCommandHandler(
    IIdentityService identityService,
    IDefaultExpenseCategoryService defaultExpenseCategories,
    TimeProvider time)
    : ICommandHandler<RegisterCommand, UserProfileDto>
{
    public async Task<UserProfileDto> HandleAsync(RegisterCommand request, CancellationToken cancellationToken = default)
    {
        var acceptedAt = time.GetUtcNow();
        var profile = await identityService.RegisterAsync(
            request.Email,
            request.Password,
            request.DisplayName,
            request.DateOfBirth,
            request.PhoneNumber,
            acceptedAt,
            acceptedAt,
            cancellationToken);

        await defaultExpenseCategories.EnsureDefaultsAsync(profile.UserId, cancellationToken);
        return profile;
}
}