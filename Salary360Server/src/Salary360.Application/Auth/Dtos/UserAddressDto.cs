namespace Salary360.Application.Auth.Dtos;

public sealed record UserAddressDto(
    string PostalCode,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string Country);
