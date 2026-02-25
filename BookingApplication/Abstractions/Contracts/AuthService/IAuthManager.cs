namespace BookingApplication.Abstractions.Contracts.AuthService;
using BookingDomain.Entities;

public interface IAuthManager
{
    string GenerateToken(Users user);
}