using MediatR;

namespace BookingApplication.Features.Properties.CreateProperty;

public class CreatePropertyCommand : IRequest<Guid>
{
    public Guid OwnerId { get; set; }
    public CreatePropertyDto CreatePropertyDto { get; set; } = null!;
}