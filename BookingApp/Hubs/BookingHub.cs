using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BookingApp.Hubs;

[Authorize]
public class BookingHub : Hub
{
}

