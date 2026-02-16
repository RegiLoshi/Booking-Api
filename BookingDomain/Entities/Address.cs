namespace BookingDomain;

using System.ComponentModel.DataAnnotations;

public class Address
{
    [Key]
    public int Id { get; set; }
    public string Country { get; set; }
    public string City { get; set; } 
    public string Street { get; set; } 
    public string ZipCode { get; set; } 

    public ICollection<Properties> Properties { get; set; } = new List<Properties>();
}
