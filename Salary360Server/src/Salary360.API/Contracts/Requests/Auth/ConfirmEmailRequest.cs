namespace Salary360.API.Contracts.Requests.Auth;

public sealed record ConfirmEmailRequest(Guid UserId, string Token);
