namespace Salary360.API.Interfaces;

public interface ILoginAttemptLimiter
{
    void Acquire(string ipAddress, string email);
}
