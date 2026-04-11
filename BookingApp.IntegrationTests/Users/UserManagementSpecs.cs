using BookingApp.IntegrationTests.Infrastructure;
using Xunit;

namespace BookingApp.IntegrationTests.Users;

[Collection(PropertyApiCollection.Name)]
public sealed class UserManagementSpecs
{
    [Fact]
    public Task User_can_update_profile_information()
        => UserManagementScenarios.User_can_update_profile_information();

    [Fact]
    public Task User_can_change_password_and_login_with_the_new_one()
        => UserManagementScenarios.User_can_change_password_and_login_with_the_new_one();
}
