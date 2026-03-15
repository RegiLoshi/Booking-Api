namespace BookingApplication.Abstractions.Contracts.Repositories;

using BookingDomain.Entities;
using BookingDomain.Repositories;

public interface IReviewRepository : IRepository<Reviews>
{
    Task<bool> ExistsForBooking(Guid bookingId, CancellationToken cancellationToken = default);
    Task<double?> GetAverageRatingForProperty(Guid propertyId, CancellationToken cancellationToken = default);
}

