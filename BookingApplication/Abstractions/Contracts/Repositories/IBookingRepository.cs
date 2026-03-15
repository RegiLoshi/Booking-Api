namespace BookingApplication.Abstractions.Contracts.Repositories;
using BookingDomain.Entities;
using BookingDomain.Repositories;

public interface IBookingRepository : IRepository<Bookings>
{
    
}