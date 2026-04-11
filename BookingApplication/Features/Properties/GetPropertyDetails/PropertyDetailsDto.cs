using BookingDomain.Entities;

namespace BookingApplication.Features.Properties.GetPropertyDetails;

public class PropertyDetailsDto
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;

    public decimal PricePerDay { get; set; }
    public decimal CleaningFreePerDay { get; set; }
    public int MaxGuests { get; set; }
    public TimeSpan CheckInTime { get; set; }
    public TimeSpan CheckOutTime { get; set; }
    public bool IsActive { get; set; }

    public AddressDto Address { get; set; } = new();
    public List<Amenity> Amenities { get; set; } = new();
    public List<string> ImageUrls { get; set; } = new();

    public double? AverageRating { get; set; }
    public int ReviewCount { get; set; }

    public List<BookedRangeDto> BookedRanges { get; set; } = new();
    public PricingRulesDto PricingRules { get; set; } = new();
    public List<ReviewPreviewDto> RecentReviews { get; set; } = new();

    public class AddressDto
    {
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
    }

    public class BookedRangeDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public BookingStatus BookingStatus { get; set; }
    }

    public class PricingRulesDto
    {
        public decimal PricePerDay { get; set; }
        public decimal CleaningFeePerDay { get; set; }
        public decimal AmenitiesUpChargePerNight { get; set; }
    }

    public class ReviewPreviewDto
    {
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid GuestId { get; set; }
        public string GuestName { get; set; } = string.Empty;
    }
}

