using System.Net;
using BookingApp.IntegrationTests.Apis;
using BookingApp.IntegrationTests.Infrastructure;
using BookingApp.IntegrationTests.Seeders;
using BookingApplication.Features.Properties.CreateProperty;
using BookingApplication.Features.Properties.GetPropertyDetails;
using BookingApplication.Features.Properties.UpdateProperty;
using BookingDomain.Entities;
using BookingInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BookingApp.IntegrationTests.Properties;

public static class PropertyEndpointsScenarios
{
    public static async Task Owner_can_create_property()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            var propertyId = await PropertyApi.CreateAsync(factory, factory.Owner, new CreatePropertyDto
            {
                Name = "Created Property",
                Description = "Created through property endpoint",
                PropertyType = "Apartment",
                PricePerDay = 150m,
                CleaningFreePerDay = 20m,
                MaxGuests = 4,
                CheckInTime = new TimeSpan(14, 0, 0),
                CheckOutTime = new TimeSpan(10, 0, 0),
                IsActive = true,
                Amenities = new List<Amenity> { Amenity.WiFi, Amenity.Parking },
                ImageUrls = new List<string> { "https://example.com/create.jpg" },
                address = new CreatePropertyDto.AddressDto
                {
                    Country = "Albania",
                    City = "Tirane",
                    Street = $"Create Street {Guid.NewGuid():N}",
                    ZipCode = "1001"
                }
            });

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
            var property = await db.Properties.FindAsync(propertyId);

            Assert.NotNull(property);
            Assert.Equal(factory.Owner.Id, property!.OwnerId);
            Assert.Equal("Created Property", property.Name);
            Assert.False(property.IsApproved);
        });
    }

    public static async Task Owner_can_update_property()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            var propertyId = await PropertySeeder.CreateAsync(factory, isApproved: false);
            await PropertyApi.UpdateAsync(factory, factory.Owner, propertyId, new UpdatePropertyDto
            {
                Name = "Updated Property",
                Description = "Updated description",
                PropertyType = "Villa",
                PricePerDay = 220m,
                CleaningFreePerDay = 30m,
                MaxGuests = 6,
                CheckInTime = new TimeSpan(15, 0, 0),
                CheckOutTime = new TimeSpan(11, 0, 0),
                IsActive = true,
                Amenities = new List<Amenity> { Amenity.SwimmingPool, Amenity.WiFi },
                ImageUrls = new List<string> { "https://example.com/updated.jpg" },
                Address = new UpdatePropertyDto.UpdateAddressDto
                {
                    Country = "Albania",
                    City = "Durres",
                    Street = $"Updated Street {Guid.NewGuid():N}",
                    ZipCode = "2001"
                }
            });

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
            var property = await db.Properties.FindAsync(propertyId);

            Assert.NotNull(property);
            Assert.Equal("Updated Property", property!.Name);
            Assert.Equal("Villa", property.PropertyType);
            Assert.Equal(220m, property.PricePerDay);
            Assert.Equal(6, property.MaxGuests);
        });
    }

    public static async Task Property_create_and_update_persist_multiple_image_urls()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            var createdImageUrls = new List<string>
            {
                "https://images.example.com/properties/create-1.jpg",
                "https://images.example.com/properties/create-2.jpg"
            };

            var propertyId = await PropertyApi.CreateAsync(factory, factory.Owner, new CreatePropertyDto
            {
                Name = "Gallery Property",
                Description = "Property with multiple photos",
                PropertyType = "Apartment",
                PricePerDay = 180m,
                CleaningFreePerDay = 25m,
                MaxGuests = 4,
                CheckInTime = new TimeSpan(14, 0, 0),
                CheckOutTime = new TimeSpan(11, 0, 0),
                IsActive = true,
                Amenities = new List<Amenity> { Amenity.WiFi },
                ImageUrls = createdImageUrls,
                address = new CreatePropertyDto.AddressDto
                {
                    Country = "Albania",
                    City = "Tirane",
                    Street = $"Gallery Street {Guid.NewGuid():N}",
                    ZipCode = "1001"
                }
            });

            using (var createScope = factory.Services.CreateScope())
            {
                var db = createScope.ServiceProvider.GetRequiredService<BookingDbContext>();
                var createdProperty = await db.Properties.FindAsync(propertyId);

                Assert.NotNull(createdProperty);
                Assert.Equal(createdImageUrls, createdProperty!.ImageUrls);
            }

            var updatedImageUrls = new List<string>
            {
                "https://images.example.com/properties/update-1.jpg",
                "https://images.example.com/properties/update-2.jpg",
                "https://images.example.com/properties/update-3.jpg"
            };

            await PropertyApi.UpdateAsync(factory, factory.Owner, propertyId, new UpdatePropertyDto
            {
                Name = "Gallery Property Updated",
                Description = "Updated property with multiple photos",
                PropertyType = "Apartment",
                PricePerDay = 195m,
                CleaningFreePerDay = 25m,
                MaxGuests = 4,
                CheckInTime = new TimeSpan(14, 0, 0),
                CheckOutTime = new TimeSpan(11, 0, 0),
                IsActive = true,
                Amenities = new List<Amenity> { Amenity.WiFi, Amenity.Parking },
                ImageUrls = updatedImageUrls,
                Address = new UpdatePropertyDto.UpdateAddressDto
                {
                    Country = "Albania",
                    City = "Tirane",
                    Street = $"Gallery Updated Street {Guid.NewGuid():N}",
                    ZipCode = "1002"
                }
            });

            using var updateScope = factory.Services.CreateScope();
            var updatedDb = updateScope.ServiceProvider.GetRequiredService<BookingDbContext>();
            var updatedProperty = await updatedDb.Properties.FindAsync(propertyId);

            Assert.NotNull(updatedProperty);
            Assert.Equal(updatedImageUrls, updatedProperty!.ImageUrls);
        });
    }

    public static async Task Public_can_get_property_details_for_visible_property()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            var propertyId = await PropertySeeder.CreateAsync(factory, isApproved: true);
            var payload = await PropertyApi.GetDetailsAsync(factory, propertyId);

            Assert.NotNull(payload);
            Assert.Equal(propertyId, payload!.Id);
            Assert.Equal(factory.Owner.Id, payload.OwnerId);
            Assert.Equal("Seeded Property", payload.Name);
            Assert.NotEmpty(payload.ImageUrls);
        });
    }

    public static async Task Public_get_property_details_returns_not_found_for_hidden_property()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            var propertyId = await PropertySeeder.CreateAsync(factory, isApproved: false);
            var status = await PropertyApi.GetDetailsStatusAsync(factory, propertyId);
            Assert.Equal(HttpStatusCode.NotFound, status);
        });
    }
}
