namespace Salary360.API.Contracts.Requests.Auth;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string DisplayName,
    DateOnly DateOfBirth,
    string? PhoneNumber,
    bool AcceptedTerms,
    bool AcceptedPrivacyPolicy);
