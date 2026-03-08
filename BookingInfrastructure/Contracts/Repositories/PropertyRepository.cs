namespace BookingInfrastructure.Contracts.Repositories;

using BookingDomain.Entities;
using BookingApplication.Abstractions.Contracts.Repositories;
using Microsoft.EntityFrameworkCore;

public class PropertyRepository : IPropertyRepository
{
    private readonly BookingDbContext _context;

    public PropertyRepository(BookingDbContext context)
    {
        _context = context;
    }
    
    // IRepository<Properties> methods
    public async Task<Properties?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Properties.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<Properties> Add(Properties entity, CancellationToken cancellationToken = default)
    {
        await _context.Properties.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<Properties> Update(Properties entity, CancellationToken cancellationToken = default)
    {
        _context.Properties.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task Delete(Properties entity, CancellationToken cancellationToken = default)
    {
        _context.Properties.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}