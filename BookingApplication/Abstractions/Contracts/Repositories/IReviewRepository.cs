namespace BookingApplication.Abstractions.Contracts.Repositories;

using BookingDomain.Entities;
using BookingDomain.Repositories;

public interface IReviewRepository : IRepository<Reviews>
{
    Task<double?> GetAverageRatingForProperty(Guid propertyId, CancellationToken cancellationToken = default);
}

