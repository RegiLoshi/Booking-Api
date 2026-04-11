using BookingApp.IntegrationTests.Infrastructure;
using Xunit;

namespace BookingApp.IntegrationTests.Admin;

[Collection(PropertyApiCollection.Name)]
public sealed class AdminEndpointsSpecs
{
    [Fact]
    public Task Admin_can_view_users()
        => AdminEndpointsScenarios.Admin_can_view_users();

    [Fact]
    public Task Admin_can_suspend_and_delete_users()
        => AdminEndpointsScenarios.Admin_can_suspend_and_delete_users();

    [Fact]
    public Task Admin_can_approve_reject_and_suspend_properties()
        => AdminEndpointsScenarios.Admin_can_approve_reject_and_suspend_properties();

    [Fact]
    public Task Admin_can_view_all_bookings()
        => AdminEndpointsScenarios.Admin_can_view_all_bookings();
}
