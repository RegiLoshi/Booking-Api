namespace BookingDomain;

using System.ComponentModel.DataAnnotations;

public class OwnerProfiles
{
    [Key]
    public Guid UserId { get; set; }
    public string IdentityCardNumber { get; set; } = string.Empty;
    public bool VerificationStatus { get; set; }
    public string BussinessName { get; set; } = string.Empty;
    public string CreditCardNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }

    public Users User { get; set; } = null!;
}