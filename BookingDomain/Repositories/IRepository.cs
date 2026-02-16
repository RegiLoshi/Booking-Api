namespace BookingDomain.Repositories;

using System;

public interface IRepository<T> where T : class
{
    Task<T?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<T> Add(T entity, CancellationToken cancellationToken = default);
    Task<T> Update(T entity, CancellationToken cancellationToken = default);
    Task Delete(T entity, CancellationToken cancellationToken = default);
}