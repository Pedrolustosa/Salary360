using Salary360.Domain.Common;

namespace Salary360.Domain.Entities;

public sealed class UserAddress : Entity
{
    private UserAddress()
    {
    }

    public UserAddress(
        Guid userId,
        string postalCode,
        string street,
        string number,
        string? complement,
        string neighborhood,
        string city,
        string state,
        string country)
    {
        UserId = userId;
        Update(postalCode, street, number, complement, neighborhood, city, state, country);
    }

    public Guid UserId { get; private set; }

    public string PostalCode { get; private set; } = string.Empty;

    public string Street { get; private set; } = string.Empty;

    public string Number { get; private set; } = string.Empty;

    public string? Complement { get; private set; }

    public string Neighborhood { get; private set; } = string.Empty;

    public string City { get; private set; } = string.Empty;

    public string State { get; private set; } = string.Empty;

    public string Country { get; private set; } = string.Empty;

    public void Update(
        string postalCode,
        string street,
        string number,
        string? complement,
        string neighborhood,
        string city,
        string state,
        string country)
    {
        if (string.IsNullOrWhiteSpace(postalCode) || string.IsNullOrWhiteSpace(street) ||
            string.IsNullOrWhiteSpace(number) || string.IsNullOrWhiteSpace(neighborhood) ||
            string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(state) ||
            string.IsNullOrWhiteSpace(country))
            throw new DomainException("The address is incomplete.", "address_incomplete");

        PostalCode = postalCode.Trim();
        Street = street.Trim();
        Number = number.Trim();
        Complement = string.IsNullOrWhiteSpace(complement) ? null : complement.Trim();
        Neighborhood = neighborhood.Trim();
        City = city.Trim();
        State = state.Trim();
        Country = country.Trim().ToUpperInvariant();
    }
}
