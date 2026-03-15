namespace BookingInfrastructure.Contracts.Repositories;

using BookingDomain.Entities;
using BookingApplication.Abstractions.Contracts.Repositories;
using Microsoft.EntityFrameworkCore;

public class ReviewRepository(BookingDbContext _context) : IReviewRepository
{
    public async Task<Reviews?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Reviews> Add(Reviews entity, CancellationToken cancellationToken = default)
    {
        await _context.Reviews.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<Reviews> Update(Reviews entity, CancellationToken cancellationToken = default)
    {
        _context.Reviews.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task Delete(Reviews entity, CancellationToken cancellationToken = default)
    {
        _context.Reviews.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsForBooking(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return await _context.Reviews.AnyAsync(r => r.BookingId == bookingId, cancellationToken);
    }

    public async Task<double?> GetAverageRatingForProperty(Guid propertyId, CancellationToken cancellationToken = default)
    {
        return await _context.Reviews
            .Where(r => r.Booking.PropertyId == propertyId)
            .Select(r => (double?)r.Rating)
            .AverageAsync(cancellationToken);
    }
}

