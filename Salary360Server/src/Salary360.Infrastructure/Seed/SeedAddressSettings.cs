namespace Salary360.Infrastructure.Seed;

public sealed class SeedAddressSettings
{
    public string PostalCode { get; set; } = string.Empty;

    public string Street { get; set; } = string.Empty;

    public string Number { get; set; } = string.Empty;

    public string? Complement { get; set; }

    public string Neighborhood { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public bool IsComplete =>
        !string.IsNullOrWhiteSpace(PostalCode) && !string.IsNullOrWhiteSpace(Street) &&
        !string.IsNullOrWhiteSpace(Number) && !string.IsNullOrWhiteSpace(Neighborhood) &&
        !string.IsNullOrWhiteSpace(City) && !string.IsNullOrWhiteSpace(State) &&
        !string.IsNullOrWhiteSpace(Country);
}