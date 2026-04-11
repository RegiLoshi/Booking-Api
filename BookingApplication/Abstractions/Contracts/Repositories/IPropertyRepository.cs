namespace BookingApplication.Abstractions.Contracts.Repositories;
using BookingDomain.Entities;
using BookingDomain.Repositories;
using BookingApplication.Features.Properties.GetPropertyDetails;
using BookingApplication.Features.Properties.SearchProperties;

public interface IPropertyRepository : IRepository<Properties>{
    Task<SearchPropertiesResponse> Search(SearchPropertiesRequest request, CancellationToken cancellationToken = default);
    Task<PropertyDetailsDto?> GetDetails(Guid propertyId, CancellationToken cancellationToken = default);
    Task<List<Properties>> GetAllProperties(CancellationToken cancellationToken = default);
    Task<bool> SetApprovalStatus(Guid propertyId, bool isApproved, CancellationToken cancellationToken = default);
    Task<bool> SetPropertyActiveStatus(Guid propertyId, bool isActive, CancellationToken cancellationToken = default);
}
