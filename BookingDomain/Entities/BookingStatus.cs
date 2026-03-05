namespace BookingDomain.Entities;

public enum BookingStatus
{
    Pending = 1,
    Confirmed,
    Rejected,
    Cancelled,
    Completed,
    Expired
}