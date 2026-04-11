using MediatR;

namespace BookingApplication.Features.Properties.GetPropertyDetails;

public class GetPropertyDetailsQuery : IRequest<PropertyDetailsDto?>
{
    public Guid PropertyId { get; set; }
}

