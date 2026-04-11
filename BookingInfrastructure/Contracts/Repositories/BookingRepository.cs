namespace BookingInfrastructure.Contracts.Repositories;
using BookingDomain.Entities;
using BookingApplication.Abstractions.Contracts.Repositories;
using Microsoft.EntityFrameworkCore;

public class BookingRepository(BookingDbContext _context) : IBookingRepository
{
    // IRepository<Bookings> methods
    public async Task<Bookings?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<Bookings> Add(Bookings entity, CancellationToken cancellationToken = default)
    {
        await _context.Bookings.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<Bookings> Update(Bookings entity, CancellationToken cancellationToken = default)
    {
        _context.Bookings.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task Delete(Bookings entity, CancellationToken cancellationToken = default)
    {
        _context.Bookings.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasOverlappingBooking(Guid propertyId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        // Overlap condition: newStart < existing.EndDate && newEnd > existing.StartDate
        return await _context.Bookings
            .AnyAsync(b =>
                b.PropertyId == propertyId &&
                (b.BookingStatus == BookingStatus.Pending || b.BookingStatus == BookingStatus.Confirmed) &&
                startDate < b.EndDate && endDate > b.StartDate,
                cancellationToken);
    }

    public async Task<List<Bookings>> GetAllBookings(CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Property)
            .Include(b => b.Guest)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }

}
