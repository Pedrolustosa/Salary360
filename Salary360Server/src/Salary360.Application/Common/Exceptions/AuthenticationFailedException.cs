namespace Salary360.Application.Common.Exceptions;

public sealed class AuthenticationFailedException(string message = "Invalid credentials.") : Exception(message);
