using BookingApplication.Abstractions.Contracts.Logging;
using LoginLogContracts;

namespace BookingInfrastructure.Logging;

public class NoOpLoginEventPublisher : ILoginEventPublisher
{
    public Task PublishAsync(LoginSucceededLogMessage message, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
