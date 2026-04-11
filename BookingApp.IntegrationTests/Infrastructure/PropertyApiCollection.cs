namespace BookingApp.IntegrationTests.Infrastructure;

using Xunit;

[CollectionDefinition(Name)]
public sealed class PropertyApiCollection : ICollectionFixture<CustomWebApplicationFactory>
{
    public const string Name = "Property API collection";
}
