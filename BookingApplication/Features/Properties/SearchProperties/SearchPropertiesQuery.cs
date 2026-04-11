using MediatR;

namespace BookingApplication.Features.Properties.SearchProperties;

public class SearchPropertiesQuery : IRequest<SearchPropertiesResponse>
{
    public SearchPropertiesRequest Request { get; set; } = new();
}

