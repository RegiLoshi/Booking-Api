using BookingApp.IntegrationTests.Apis;
using BookingApp.IntegrationTests.Infrastructure;
using BookingApp.IntegrationTests.Seeders;
using BookingDomain.Entities;
using BookingInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BookingApp.IntegrationTests.Admin;

public static class AdminEndpointsScenarios
{
    public static async Task Admin_can_view_users()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            var users = await AdminApi.GetUsersAsync<AdminUserDto>(factory);

            Assert.Contains(users, u => u.Id == factory.Owner.Id);
            Assert.Contains(users, u => u.Id == factory.Client.Id);
            Assert.Contains(users, u => u.Id == factory.Admin.Id && u.Roles.Contains("Admin"));
        });
    }

    public static async Task Admin_can_suspend_and_delete_users()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            await AdminApi.SuspendUserAsync(factory, factory.Client.Id);

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
                var suspendedUser = await db.Users.FindAsync(factory.Client.Id);
                Assert.NotNull(suspendedUser);
                Assert.False(suspendedUser!.IsActive);
            }

            await AdminApi.DeleteUserAsync(factory, factory.Owner.Id);

            using var verifyScope = factory.Services.CreateScope();
            var verifyDb = verifyScope.ServiceProvider.GetRequiredService<BookingDbContext>();
            var deletedUser = await verifyDb.Users.FindAsync(factory.Owner.Id);
            Assert.Null(deletedUser);
        });
    }

    public static async Task Admin_can_approve_reject_and_suspend_properties()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            var propertyToApprove = await PropertySeeder.CreateAsync(factory, name: "Approve Me", isApproved: false);
            var propertyToReject = await PropertySeeder.CreateAsync(factory, name: "Reject Me", isApproved: true);
            var propertyToSuspend = await PropertySeeder.CreateAsync(factory, name: "Suspend Me", isApproved: true, isActive: true);

            var properties = await AdminApi.GetPropertiesAsync<AdminPropertyDto>(factory);
            Assert.Contains(properties, p => p.Id == propertyToApprove);
            Assert.Contains(properties, p => p.Id == propertyToReject);
            Assert.Contains(properties, p => p.Id == propertyToSuspend);

            await AdminApi.ApprovePropertyAsync(factory, propertyToApprove);
            await AdminApi.RejectPropertyAsync(factory, propertyToReject);
            await AdminApi.SuspendPropertyAsync(factory, propertyToSuspend);

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();

            var approved = await db.Properties.FindAsync(propertyToApprove);
            var rejected = await db.Properties.FindAsync(propertyToReject);
            var suspended = await db.Properties.FindAsync(propertyToSuspend);

            Assert.NotNull(approved);
            Assert.True(approved!.IsApproved);

            Assert.NotNull(rejected);
            Assert.False(rejected!.IsApproved);
            Assert.False(rejected.IsActive);

            Assert.NotNull(suspended);
            Assert.False(suspended!.IsActive);
        });
    }

    public static async Task Admin_can_view_all_bookings()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            var propertyId = await PropertySeeder.CreateAsync(factory, name: "Oversight Property", isApproved: true);
            var bookingId = await BookingSeeder.CreateAsync(factory, propertyId, status: BookingStatus.Pending);

            var bookings = await AdminApi.GetBookingsAsync<AdminBookingDto>(factory);

            Assert.Contains(bookings, b => b.Id == bookingId && b.PropertyId == propertyId && b.GuestId == factory.Client.Id);
        });
    }

    private sealed class AdminUserDto
    {
        public Guid Id { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    private sealed class AdminPropertyDto
    {
        public Guid Id { get; set; }
    }

    private sealed class AdminBookingDto
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public Guid GuestId { get; set; }
    }
}
