using LoginLogContracts;

namespace BookingApplication.Abstractions.Contracts.Logging;

public interface ILoginEventPublisher
{
    Task PublishAsync(LoginSucceededLogMessage message, CancellationToken cancellationToken = default);
}
