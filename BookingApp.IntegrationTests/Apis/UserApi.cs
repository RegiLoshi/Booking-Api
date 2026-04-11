using System.Net.Http.Json;
using BookingApp.IntegrationTests.Infrastructure;
using BookingApplication.Features.Users.ChangePassword;
using BookingApplication.Features.Users.Login;
using BookingApplication.Features.Users.UpdateUser;

namespace BookingApp.IntegrationTests.Apis;

public static class UserApi
{
    public static async Task UpdateAsync(CustomWebApplicationFactory factory, UpdateUserDto dto)
    {
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/v1/user/update", new UpdateUserCommand
        {
            UpdateUserDto = dto
        });
        response.EnsureSuccessStatusCode();
    }

    public static async Task ChangePasswordAsync(CustomWebApplicationFactory factory, UserChangePasswordDto dto)
    {
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/v1/user/changePassword", new UserChangePasswordCommand
        {
            userChangePasswordDto = dto
        });
        response.EnsureSuccessStatusCode();
    }

    public static async Task<HttpResponseMessage> LoginRawAsync(CustomWebApplicationFactory factory, string email, string password)
    {
        using var client = factory.CreateClient();
        return await client.PostAsJsonAsync("/v1/user/login", new LogInUserCommand
        {
            LogInUserDto = new LogInUserDto
            {
                Email = email,
                Password = password
            }
        });
    }

    public static async Task<LogInUserResponse?> LoginAsync(CustomWebApplicationFactory factory, string email, string password)
    {
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/v1/user/login", new LogInUserCommand
        {
            LogInUserDto = new LogInUserDto
            {
                Email = email,
                Password = password
            }
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LogInUserResponse>();
    }
}
