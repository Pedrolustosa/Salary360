using Microsoft.EntityFrameworkCore;
using Salary360.Application.Interfaces.Identity;
using Salary360.Domain.Enums;
using Salary360.Infrastructure.Persistence;

namespace Salary360.Infrastructure.Identity;

public sealed class UserSessionValidator(AppDbContext dbContext) : IUserSessionValidator
{
    public Task<bool> IsActiveAsync(Guid userId, CancellationToken cancellationToken = default)
        => dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.Id == userId && user.Status == UserStatus.Active, cancellationToken);
}
