using System.Net;
using BookingApp.IntegrationTests.Apis;
using BookingApp.IntegrationTests.Infrastructure;
using BookingApp.IntegrationTests.Seeders;
using BookingApplication.Features.Properties.SearchProperties;
using BookingDomain.Entities;
using Xunit;

namespace BookingApp.IntegrationTests.Properties;

public static class PropertySearchScenarios
{
    public static async Task Public_search_returns_only_publicly_visible_properties()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            var visible = await PropertySeeder.CreateAsync(factory, name: "Visible Apartment", city: "Tirane", isActive: true, isApproved: true);
            await PropertySeeder.CreateAsync(factory, name: "Inactive Apartment", city: "Tirane", isActive: false, isApproved: true);
            await PropertySeeder.CreateAsync(factory, name: "Unapproved Apartment", city: "Tirane", isActive: true, isApproved: false);

            var result = await PropertyApi.SearchAsync(factory, "/v1/property/search?sort=4&page=1&pageSize=20");

            Assert.Contains(result.Items, x => x.Id == visible);
            Assert.DoesNotContain(result.Items, x => x.Name == "Inactive Apartment");
            Assert.DoesNotContain(result.Items, x => x.Name == "Unapproved Apartment");
        });
    }

    public static async Task Search_can_filter_by_city()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            var tirane = await PropertySeeder.CreateAsync(factory, name: "Tirane Apartment", city: "Tirane");
            await PropertySeeder.CreateAsync(factory, name: "Vlore Apartment", city: "Vlore");

            var result = await PropertyApi.SearchAsync(factory, "/v1/property/search?city=Tirane&sort=4&page=1&pageSize=20");

            Assert.Contains(result.Items, x => x.Id == tirane);
            Assert.DoesNotContain(result.Items, x => x.City == "Vlore");
        });
    }

    public static async Task Search_can_filter_by_property_type_and_guests()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            var villa = await PropertySeeder.CreateAsync(factory, name: "Large Villa", propertyType: "Villa", maxGuests: 6);
            await PropertySeeder.CreateAsync(factory, name: "Small Apartment", propertyType: "Apartment", maxGuests: 2);
            await PropertySeeder.CreateAsync(factory, name: "Tiny Villa", propertyType: "Villa", maxGuests: 2);

            var result = await PropertyApi.SearchAsync(factory, "/v1/property/search?propertyType=Villa&guests=4&sort=4&page=1&pageSize=20");

            Assert.Contains(result.Items, x => x.Id == villa);
            Assert.All(result.Items, x =>
            {
                Assert.Equal("Villa", x.PropertyType);
                Assert.True(x.MaxGuests >= 4);
            });
        });
    }

    public static async Task Search_can_filter_by_price_range()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            await PropertySeeder.CreateAsync(factory, name: "Budget Stay", pricePerDay: 60m);
            var midRange = await PropertySeeder.CreateAsync(factory, name: "Mid Stay", pricePerDay: 120m);
            await PropertySeeder.CreateAsync(factory, name: "Luxury Stay", pricePerDay: 240m);

            var result = await PropertyApi.SearchAsync(factory, "/v1/property/search?minPricePerDay=100&maxPricePerDay=150&sort=4&page=1&pageSize=20");

            Assert.Contains(result.Items, x => x.Id == midRange);
            Assert.All(result.Items, x => Assert.InRange(x.PricePerDay, 100m, 150m));
        });
    }

    public static async Task Search_can_filter_by_amenities()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            var poolProperty = await PropertySeeder.CreateAsync(factory, name: "Pool Villa", amenities: new List<Amenity> { Amenity.SwimmingPool, Amenity.WiFi });
            await PropertySeeder.CreateAsync(factory, name: "Wifi Apartment", amenities: new List<Amenity> { Amenity.WiFi });

            var result = await PropertyApi.SearchAsync(factory, "/v1/property/search?amenities=5&sort=4&page=1&pageSize=20");

            Assert.Contains(result.Items, x => x.Id == poolProperty);
            Assert.All(result.Items, x => Assert.Contains(Amenity.SwimmingPool, x.Amenities));
        });
    }

    public static async Task Search_can_filter_by_availability_dates()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            var available = await PropertySeeder.CreateAsync(factory, name: "Available Stay", city: "Tirane");
            var booked = await PropertySeeder.CreateAsync(factory, name: "Booked Stay", city: "Tirane");
            await BookingSeeder.CreateAsync(
                factory,
                booked,
                startDate: new DateTime(2026, 5, 10),
                endDate: new DateTime(2026, 5, 15),
                status: BookingStatus.Pending);

            var result = await PropertyApi.SearchAsync(factory, "/v1/property/search?city=Tirane&startDate=2026-05-11&endDate=2026-05-13&sort=4&page=1&pageSize=20");

            Assert.Contains(result.Items, x => x.Id == available);
            Assert.DoesNotContain(result.Items, x => x.Id == booked);
        });
    }

    public static async Task Search_returns_bad_request_when_end_date_is_not_after_start_date()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            var status = await PropertyApi.SearchStatusAsync(factory, "/v1/property/search?startDate=2026-05-10&endDate=2026-05-10");
            Assert.Equal(HttpStatusCode.BadRequest, status);
        });
    }
}
