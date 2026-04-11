using BookingApp.IntegrationTests.Infrastructure;
using BookingDomain.Entities;
using BookingInfrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace BookingApp.IntegrationTests.Seeders;

public static class PropertySeeder
{
    public static async Task<Guid> CreateAsync(
        CustomWebApplicationFactory factory,
        string name = "Seeded Property",
        string city = "Tirane",
        string propertyType = "Apartment",
        decimal pricePerDay = 120m,
        decimal cleaningFeePerDay = 15m,
        int maxGuests = 4,
        bool isActive = true,
        bool isApproved = true,
        List<Amenity>? amenities = null,
        string description = "Seeded property for tests")
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();

        var address = new Address
        {
            Id = Guid.NewGuid(),
            Country = "Albania",
            City = city,
            Street = $"{name}-{Guid.NewGuid():N}",
            ZipCode = city == "Durres" ? "2001" : "1001"
        };

        var property = new BookingDomain.Entities.Properties
        {
            Id = Guid.NewGuid(),
            OwnerId = factory.Owner.Id,
            Name = name,
            Description = description,
            PropertyType = propertyType,
            PricePerDay = pricePerDay,
            CleaningFreePerDay = cleaningFeePerDay,
            AddressId = address.Id,
            MaxGuests = maxGuests,
            CheckInTime = new TimeSpan(14, 0, 0),
            CheckOutTime = new TimeSpan(10, 0, 0),
            IsActive = isActive,
            IsApproved = isApproved,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow,
            Amenities = amenities ?? new List<Amenity> { Amenity.WiFi, Amenity.Parking },
            ImageUrls = new List<string> { "https://example.com/property.jpg" }
        };

        db.Addresses.Add(address);
        db.Properties.Add(property);
        await db.SaveChangesAsync();

        return property.Id;
    }
}
