using System.Net.Http.Json;
using BookingApp.IntegrationTests.Apis;
using BookingApp.IntegrationTests.Infrastructure;
using BookingApp.IntegrationTests.Seeders;
using BookingApplication.Features.Booking.CreateBooking;
using BookingDomain.Entities;
using BookingInfrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BookingApp.IntegrationTests.Booking;

public static class BookingEndpointsScenarios
{
    public static async Task Client_can_create_a_booking_that_starts_as_pending()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            var propertyId = await PropertySeeder.CreateAsync(factory, name: $"Booking Property {Guid.NewGuid():N}");
            var startDate = DateTime.UtcNow.Date.AddDays(10);
            var endDate = startDate.AddDays(3);

            var bookingId = await BookingApi.CreateAsync(factory, factory.Client, new CreateBookingDto
            {
                PropertyId = propertyId,
                StartDate = startDate,
                EndDate = endDate,
                GuestCount = 2,
                BookingStatus = BookingStatus.Pending
            });

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
            var booking = await db.Bookings.FindAsync(bookingId);

            Assert.NotNull(booking);
            Assert.Equal(BookingStatus.Pending, booking!.BookingStatus);
            Assert.Equal(factory.Client.Id, booking.GuestId);
            Assert.Equal(propertyId, booking.PropertyId);
            Assert.Equal(45m, booking.CleaningFee);
            Assert.Equal(6m, booking.AmenitiesUpCharge);
            Assert.Equal(360m, booking.PriceForPeriod);
            Assert.Equal(411m, booking.TotalPrice);
        });
    }

    public static async Task Owner_can_confirm_a_pending_booking()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            var bookingId = await CreatePendingBookingAsync(factory);
            await BookingApi.ConfirmAsync(factory, factory.Owner, bookingId);

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
            var booking = await db.Bookings.FindAsync(bookingId);

            Assert.NotNull(booking);
            Assert.Equal(BookingStatus.Confirmed, booking!.BookingStatus);
            Assert.NotNull(booking.ConfirmedOnUtc);
        });
    }

    public static async Task Owner_can_reject_a_pending_booking()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            var bookingId = await CreatePendingBookingAsync(factory);
            await BookingApi.RejectAsync(factory, factory.Owner, bookingId);

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
            var booking = await db.Bookings.FindAsync(bookingId);

            Assert.NotNull(booking);
            Assert.Equal(BookingStatus.Rejected, booking!.BookingStatus);
            Assert.NotNull(booking.RejectedOnUtc);
        });
    }

    public static async Task Guest_can_cancel_a_pending_booking()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            var bookingId = await CreatePendingBookingAsync(factory);
            await BookingApi.CancelAsync(factory, factory.Client, bookingId);

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
            var booking = await db.Bookings.FindAsync(bookingId);

            Assert.NotNull(booking);
            Assert.Equal(BookingStatus.Cancelled, booking!.BookingStatus);
            Assert.NotNull(booking.CancelledOnUtc);
        });
    }

    public static async Task System_can_mark_a_booking_as_completed()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            var bookingId = await CreatePendingBookingAsync(factory);
            await BookingSeeder.UpdateStatusAsync(factory, bookingId, BookingStatus.Confirmed);
            await BookingApi.CompleteAsync(factory, factory.Owner, bookingId);

            using var verificationScope = factory.Services.CreateScope();
            var verificationDb = verificationScope.ServiceProvider.GetRequiredService<BookingDbContext>();
            var updatedBooking = await verificationDb.Bookings.FindAsync(bookingId);

            Assert.NotNull(updatedBooking);
            Assert.Equal(BookingStatus.Completed, updatedBooking!.BookingStatus);
            Assert.NotNull(updatedBooking.CompletedOnUtc);
        });
    }

    public static async Task Expired_status_can_be_persisted_for_system_managed_bookings()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            var bookingId = await CreatePendingBookingAsync(factory);
            await BookingSeeder.UpdateStatusAsync(factory, bookingId, BookingStatus.Expired);

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
            var persistedBooking = await db.Bookings.AsNoTracking().FirstAsync(b => b.Id == bookingId);
            Assert.Equal(BookingStatus.Expired, persistedBooking.BookingStatus);
        });
    }

    private static async Task<Guid> CreatePendingBookingAsync(CustomWebApplicationFactory factory)
    {
        var propertyId = await PropertySeeder.CreateAsync(factory, name: $"Booking Property {Guid.NewGuid():N}");
        var startDate = DateTime.UtcNow.Date.AddDays(10);
        var endDate = startDate.AddDays(3);

        return await BookingApi.CreateAsync(factory, factory.Client, new CreateBookingDto
        {
            PropertyId = propertyId,
            StartDate = startDate,
            EndDate = endDate,
            GuestCount = 2,
            BookingStatus = BookingStatus.Pending
        });
    }
}
