using BookingDomain.Entities;

namespace BookingApplication.Features.Properties.UpdateProperty;

public class UpdatePropertyDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public UpdateAddressDto? Address { get; set; }
    public int MaxGuests { get; set; }
    public TimeSpan CheckInTime { get; set; }
    public TimeSpan CheckOutTime { get; set; }
    public bool IsActive { get; set; }
    public List<Amenity> Amenities { get; set; } = new();
    public List<string> ImageUrls { get; set; } = new();

    public class UpdateAddressDto
    {
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
    }
}
