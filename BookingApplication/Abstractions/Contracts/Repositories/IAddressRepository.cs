namespace BookingApplication.Abstractions.Contracts.Repositories;

using BookingDomain.Entities;
using BookingDomain.Repositories;

public interface IAddressRepository : IRepository<Address>
{
    Task<Guid?> FindIdByStreetCityZipCountry(
        string country,
        string city,
        string street,
        string zipCode,
        CancellationToken cancellationToken = default);
}
