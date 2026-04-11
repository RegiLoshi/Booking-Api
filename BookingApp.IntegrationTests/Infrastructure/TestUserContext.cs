namespace BookingApp.IntegrationTests.Infrastructure;

public sealed class TestUserContext
{
    public required Guid Id { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Token { get; set; }
}
