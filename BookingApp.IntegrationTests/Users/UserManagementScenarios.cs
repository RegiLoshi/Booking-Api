using System.Net;
using BCrypt.Net;
using BookingApp.IntegrationTests.Apis;
using BookingApp.IntegrationTests.Infrastructure;
using BookingApplication.Features.Users.ChangePassword;
using BookingApplication.Features.Users.Login;
using BookingApplication.Features.Users.UpdateUser;
using BookingInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BookingApp.IntegrationTests.Users;

public static class UserManagementScenarios
{
    public static async Task User_can_update_profile_information()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            var dto = new UpdateUserDto
            {
                Id = factory.Client.Id,
                FirstName = "Updated",
                LastName = "Client",
                PhoneNumber = "5551234567",
                ProfilePictureUrl = "https://example.com/updated-client.png"
            };

            await UserApi.UpdateAsync(factory, dto);

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
            var updatedUser = await db.Users.FindAsync(factory.Client.Id);

            Assert.NotNull(updatedUser);
            Assert.Equal(dto.FirstName, updatedUser!.FirstName);
            Assert.Equal(dto.LastName, updatedUser.LastName);
            Assert.Equal(dto.PhoneNumber, updatedUser.PhoneNumber);
            Assert.Equal(dto.ProfilePictureUrl, updatedUser.ProfilePictureUrl);
        });
    }

    public static async Task User_can_change_password_and_login_with_the_new_one()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            const string newPassword = "Pass456!";

            await UserApi.ChangePasswordAsync(factory, new UserChangePasswordDto
            {
                Id = factory.Client.Id,
                OldPassword = factory.Client.Password,
                NewPassword = newPassword
            });

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
                var updatedUser = await db.Users.FindAsync(factory.Client.Id);

                Assert.NotNull(updatedUser);
                Assert.True(BCrypt.Net.BCrypt.EnhancedVerify(newPassword, updatedUser!.Password));
                Assert.False(BCrypt.Net.BCrypt.EnhancedVerify(factory.Client.Password, updatedUser.Password));
            }

            var oldLoginResponse = await UserApi.LoginRawAsync(factory, factory.Client.Email, factory.Client.Password);
            Assert.Equal(HttpStatusCode.InternalServerError, oldLoginResponse.StatusCode);

            var loginPayload = await UserApi.LoginAsync(factory, factory.Client.Email, newPassword);
            Assert.NotNull(loginPayload);
            Assert.Equal(factory.Client.Id, loginPayload!.Id);
            Assert.Equal(factory.Client.Email, loginPayload.Email);
            Assert.False(string.IsNullOrWhiteSpace(loginPayload.Token));
        });
    }
}
