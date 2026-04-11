using BookingApplication.Abstractions.Contracts.Repositories;
using MediatR;

namespace BookingApplication.Features.Properties.SearchProperties;

public class SearchPropertiesQueryHandler(IPropertyRepository propertyRepository)
    : IRequestHandler<SearchPropertiesQuery, SearchPropertiesResponse>
{
    public async Task<SearchPropertiesResponse> Handle(SearchPropertiesQuery request, CancellationToken cancellationToken)
    {
        var r = request.Request ?? new SearchPropertiesRequest();

        if (r.Page <= 0) r.Page = 1;
        if (r.PageSize <= 0) r.PageSize = 20;
        if (r.PageSize > 100) r.PageSize = 100;

        if (r.StartDate.HasValue && r.EndDate.HasValue && r.EndDate.Value.Date <= r.StartDate.Value.Date)
            throw new ArgumentException("EndDate must be after StartDate.");

        return await propertyRepository.Search(r, cancellationToken);
    }
}

