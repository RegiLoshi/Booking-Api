using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace BookingApp.SignalR;

public class UserIdFromClaimProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirstValue("userId");
    }
}

