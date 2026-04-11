namespace BookingApplication.Abstractions.Contracts.Repositories;
using BookingDomain.Entities;
using BookingDomain.Repositories;
using BookingApplication.Features.Properties.GetPropertyDetails;
using BookingApplication.Features.Properties.SearchProperties;

public interface IPropertyRepository : IRepository<Properties>{
    Task<SearchPropertiesResponse> Search(SearchPropertiesRequest request, CancellationToken cancellationToken = default);
    Task<PropertyDetailsDto?> GetDetails(Guid propertyId, CancellationToken cancellationToken = default);
}