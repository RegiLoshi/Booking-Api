namespace BookingInfrastructure.Contracts.Repositories;
using BookingDomain.Entities;
using BookingApplication.Abstractions.Contracts.Repositories;
using Microsoft.EntityFrameworkCore;
public class AddressRepository : IAddressRepository
{
    private readonly BookingDbContext _context;

    public AddressRepository(BookingDbContext context)
    {
        _context = context;
    }
    
    // IRepository<Address> methods
    public async Task<Address?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Addresses.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<Address> Add(Address entity, CancellationToken cancellationToken = default)
    {
        await _context.Addresses.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<Address> Update(Address entity, CancellationToken cancellationToken = default)
    {
        _context.Addresses.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task Delete(Address entity, CancellationToken cancellationToken = default)
    {
        _context.Addresses.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid?> FindIdByStreetCityZipCountry(
        string country,
        string city,
        string street,
        string zipCode,
        CancellationToken cancellationToken = default)
    {
        var c = country.Trim().ToLowerInvariant();
        var cityNorm = city.Trim().ToLowerInvariant();
        var streetNorm = street.Trim().ToLowerInvariant();
        var zipNorm = zipCode.Trim().ToLowerInvariant();

        return await _context.Addresses
            .Where(a => a.Country.Trim().ToLower() == c
                        && a.City.Trim().ToLower() == cityNorm
                        && a.Street.Trim().ToLower() == streetNorm
                        && a.ZipCode.Trim().ToLower() == zipNorm)
            .Select(a => (Guid?)a.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}