namespace Salary360.API.Contracts.Responses.Auth;

public sealed record UserAddressResponse(
    string PostalCode,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string Country);
