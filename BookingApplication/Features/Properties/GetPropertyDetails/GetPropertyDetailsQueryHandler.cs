using BookingApplication.Abstractions.Contracts.Repositories;
using MediatR;

namespace BookingApplication.Features.Properties.GetPropertyDetails;

public class GetPropertyDetailsQueryHandler(IPropertyRepository propertyRepository)
    : IRequestHandler<GetPropertyDetailsQuery, PropertyDetailsDto?>
{
    public async Task<PropertyDetailsDto?> Handle(GetPropertyDetailsQuery request, CancellationToken cancellationToken)
    {
        return await propertyRepository.GetDetails(request.PropertyId, cancellationToken);
    }
}

