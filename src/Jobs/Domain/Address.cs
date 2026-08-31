using JobTrackerService.Shared;

namespace JobTrackerService.Jobs.Domain;

public sealed class Address : ValueObject
{
    private Address() { }

    public Address(string street, string city, string state, string zipCode, decimal? latitude = null, decimal? longitude = null)
    {
        Street = street.Trim(); City = city.Trim(); State = state.Trim(); ZipCode = zipCode.Trim();
        Latitude = latitude; Longitude = longitude;
    }

    public string Street { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string ZipCode { get; private set; } = string.Empty;
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }

    protected override IEnumerable<object?> GetEqualityComponents() => [Street, City, State, ZipCode, Latitude, Longitude];
}