namespace BookingApplication.Features.Properties.SearchProperties;

public class SearchPropertiesRequest
{
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? ZipCode { get; set; }
    public string? PropertyType { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public int? Guests { get; set; }
    public decimal? MinPricePerDay { get; set; }
    public decimal? MaxPricePerDay { get; set; }

    public List<int>? Amenities { get; set; }

    public double? MinRating { get; set; }

    public PropertySearchSort Sort { get; set; } = PropertySearchSort.PriceAsc;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

