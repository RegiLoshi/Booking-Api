using BookingApp.IntegrationTests.Infrastructure;
using Xunit;

namespace BookingApp.IntegrationTests.Booking;

[Collection(PropertyApiCollection.Name)]
public sealed class BookingEndpointsSpecs
{
    [Fact]
    public Task Client_can_create_a_booking_that_starts_as_pending()
        => BookingEndpointsScenarios.Client_can_create_a_booking_that_starts_as_pending();

    [Fact]
    public Task Owner_can_confirm_a_pending_booking()
        => BookingEndpointsScenarios.Owner_can_confirm_a_pending_booking();

    [Fact]
    public Task Owner_can_reject_a_pending_booking()
        => BookingEndpointsScenarios.Owner_can_reject_a_pending_booking();

    [Fact]
    public Task Guest_can_cancel_a_pending_booking()
        => BookingEndpointsScenarios.Guest_can_cancel_a_pending_booking();

    [Fact]
    public Task System_can_mark_a_booking_as_completed()
        => BookingEndpointsScenarios.System_can_mark_a_booking_as_completed();

    [Fact]
    public Task Expired_status_can_be_persisted_for_system_managed_bookings()
        => BookingEndpointsScenarios.Expired_status_can_be_persisted_for_system_managed_bookings();
}
