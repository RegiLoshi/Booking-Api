using BookingApp.IntegrationTests.Infrastructure;
using Xunit;

namespace BookingApp.IntegrationTests.Reviews;

[Collection(PropertyApiCollection.Name)]
public sealed class ReviewEndpointsSpecs
{
    [Fact]
    public Task Guest_can_create_review_for_completed_booking()
        => ReviewEndpointsScenarios.Guest_can_create_review_for_completed_booking();

    [Fact]
    public Task Property_average_rating_is_returned_for_reviews()
        => ReviewEndpointsScenarios.Property_average_rating_is_returned_for_reviews();
}
