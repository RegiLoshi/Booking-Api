using BookingApp.IntegrationTests.Infrastructure;
using Xunit;

namespace BookingApp.IntegrationTests.Properties;

[Collection(PropertyApiCollection.Name)]
public sealed class PropertySearchSpecs
{
    [Fact]
    public Task Public_search_returns_only_publicly_visible_properties()
        => PropertySearchScenarios.Public_search_returns_only_publicly_visible_properties();

    [Fact]
    public Task Search_can_filter_by_city()
        => PropertySearchScenarios.Search_can_filter_by_city();

    [Fact]
    public Task Search_can_filter_by_property_type_and_guests()
        => PropertySearchScenarios.Search_can_filter_by_property_type_and_guests();

    [Fact]
    public Task Search_can_filter_by_price_range()
        => PropertySearchScenarios.Search_can_filter_by_price_range();

    [Fact]
    public Task Search_can_filter_by_amenities()
        => PropertySearchScenarios.Search_can_filter_by_amenities();

    [Fact]
    public Task Search_can_filter_by_availability_dates()
        => PropertySearchScenarios.Search_can_filter_by_availability_dates();

    [Fact]
    public Task Search_returns_bad_request_when_end_date_is_not_after_start_date()
        => PropertySearchScenarios.Search_returns_bad_request_when_end_date_is_not_after_start_date();
}
