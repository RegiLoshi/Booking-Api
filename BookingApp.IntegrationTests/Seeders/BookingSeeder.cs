using BookingApp.IntegrationTests.Infrastructure;
using BookingDomain.Entities;
using BookingInfrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingApp.IntegrationTests.Seeders;

public static class BookingSeeder
{
    public static async Task<Guid> CreateAsync(
        CustomWebApplicationFactory factory,
        Guid propertyId,
        Guid? guestId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        BookingStatus status = BookingStatus.Pending,
        decimal cleaningFee = 30m,
        decimal amenitiesUpCharge = 0m,
        decimal priceForPeriod = 240m,
        decimal totalPrice = 270m)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();

        var booking = new Bookings
        {
            Id = Guid.NewGuid(),
            PropertyId = propertyId,
            GuestId = guestId ?? factory.Client.Id,
            StartDate = (startDate ?? DateTime.UtcNow.Date.AddDays(10)).Date,
            EndDate = (endDate ?? DateTime.UtcNow.Date.AddDays(13)).Date,
            GuestCount = 2,
            CleaningFee = cleaningFee,
            AmenitiesUpCharge = amenitiesUpCharge,
            PriceForPeriod = priceForPeriod,
            TotalPrice = totalPrice,
            BookingStatus = status,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow,
            CreatedOnUtc = DateTime.UtcNow,
            ConfirmedOnUtc = status == BookingStatus.Confirmed ? DateTime.UtcNow : null,
            RejectedOnUtc = status == BookingStatus.Rejected ? DateTime.UtcNow : null,
            CompletedOnUtc = status == BookingStatus.Completed ? DateTime.UtcNow : null,
            CancelledOnUtc = status == BookingStatus.Cancelled ? DateTime.UtcNow : null
        };

        db.Bookings.Add(booking);
        await db.SaveChangesAsync();
        return booking.Id;
    }

    public static async Task UpdateStatusAsync(CustomWebApplicationFactory factory, Guid bookingId, BookingStatus status)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
        var booking = await db.Bookings.FirstAsync(b => b.Id == bookingId);
        booking.BookingStatus = status;
        booking.LastModifiedAt = DateTime.UtcNow;
        booking.ConfirmedOnUtc = status == BookingStatus.Confirmed ? DateTime.UtcNow : booking.ConfirmedOnUtc;
        booking.CompletedOnUtc = status == BookingStatus.Completed ? DateTime.UtcNow : booking.CompletedOnUtc;
        await db.SaveChangesAsync();
    }
}
