namespace BookingInfrastructure.Repositories;

using System;
using BookingDomain.Repositories;
using BookingInfrastructure;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly BookingDbContext _context;

    public Repository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<T?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<T> Add(T entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<T>().AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<T> Update(T entity, CancellationToken cancellationToken = default)
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task Delete(T entity, CancellationToken cancellationToken = default)
    {
        _context.Set<T>().Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}