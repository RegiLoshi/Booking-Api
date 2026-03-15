using BookingDomain.Entities;

namespace BookingApplication.Features.Properties.CreateProperty;

public class CreatePropertyDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string PropertyType { get; set; }
    public decimal PricePerDay { get; set; }
    public decimal CleaningFreePerDay { get; set; }
    public AddressDto address { get; set; }
    public int MaxGuests { get; set; }
    public TimeSpan CheckInTime { get; set; }
    public TimeSpan CheckOutTime { get; set; }
    public bool IsActive { get; set; }
    public List<Amenity> Amenities { get; set; }
    public List<string> ImageUrls { get; set; } = new();

    public class AddressDto
    {
        public string Country { get; set; }
        public string City { get; set; } 
        public string Street { get; set; } 
        public string ZipCode { get; set; } 
    }
}