namespace Salary360.Application.Common;

public static class ErrorCodes
{
    public const string ValidationFailed = "validation_failed";
    public const string InvalidCredentials = "invalid_credentials";
    public const string EmailNotConfirmed = "email_not_confirmed";
    public const string UserInactive = "user_inactive";
    public const string AccountLocked = "account_locked";
    public const string Unauthorized = "unauthorized";
    public const string Forbidden = "forbidden";
    public const string NotFound = "not_found";
    public const string Conflict = "conflict";
    public const string IdentityFailed = "identity_failed";
    public const string TooManyRequests = "too_many_requests";
    public const string InternalError = "internal_error";
    public const string MalformedRequest = "malformed_request";
}