namespace BookingApplication.Features.Properties.SearchProperties;

public class SearchPropertiesResponse
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public List<PropertySearchResultDto> Items { get; set; } = new();
}

