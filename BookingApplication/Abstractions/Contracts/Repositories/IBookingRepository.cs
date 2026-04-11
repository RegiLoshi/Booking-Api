namespace BookingApplication.Abstractions.Contracts.Repositories;
using BookingDomain.Entities;
using BookingDomain.Repositories;

public interface IBookingRepository : IRepository<Bookings>
{
    Task<bool> HasOverlappingBooking(Guid propertyId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<List<Bookings>> GetAllBookings(CancellationToken cancellationToken = default);
}
