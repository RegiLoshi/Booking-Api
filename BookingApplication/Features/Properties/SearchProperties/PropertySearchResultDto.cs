using BookingDomain.Entities;

namespace BookingApplication.Features.Properties.SearchProperties;

public class PropertySearchResultDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
    public decimal CleaningFreePerDay { get; set; }
    public int MaxGuests { get; set; }
    public bool IsActive { get; set; }

    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;

    public List<Amenity> Amenities { get; set; } = new();
    public List<string> ImageUrls { get; set; } = new();

    public double? AverageRating { get; set; }
    public int ReviewCount { get; set; }
}

