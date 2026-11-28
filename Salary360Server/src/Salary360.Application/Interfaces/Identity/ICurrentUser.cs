namespace Salary360.Application.Interfaces.Identity;

public interface ICurrentUser
{
    Guid GetUserId();
}