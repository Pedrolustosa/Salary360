namespace Salary360.Application.Interfaces.Identity;

public interface IUserSessionValidator
{
    Task<bool> IsActiveAsync(Guid userId, CancellationToken cancellationToken = default);
}