using MediatR;

namespace BookingApplication.Features.Properties.UpdateProperty;

public class UpdatePropertyCommand : IRequest<bool>
{
    public Guid PropertyId { get; set; }
    public Guid OwnerId { get; set; }
    public UpdatePropertyDto UpdatePropertyDto { get; set; } = null!;
}
