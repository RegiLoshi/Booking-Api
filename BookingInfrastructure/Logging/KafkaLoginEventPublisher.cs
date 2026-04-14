using System.Text.Json;
using BookingApplication.Abstractions.Contracts.Logging;
using Confluent.Kafka;
using LoginLogContracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookingInfrastructure.Logging;

public class KafkaLoginEventPublisher(
    IOptions<BookingKafkaOptions> options,
    ILogger<KafkaLoginEventPublisher> logger) : ILoginEventPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly BookingKafkaOptions _options = options.Value;

    public async Task PublishAsync(LoginSucceededLogMessage message, CancellationToken cancellationToken = default)
    {
        if (!_options.ProducerEnabled || string.IsNullOrWhiteSpace(_options.BootstrapServers))
            return;

        var config = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers
        };

        using var producer = new ProducerBuilder<Null, string>(config).Build();

        var payload = JsonSerializer.Serialize(message, JsonOptions);
        try
        {
            await producer.ProduceAsync(
                _options.Topic,
                new Message<Null, string> { Value = payload },
                cancellationToken);
        }
        catch (ProduceException<Null, string> ex)
        {
            logger.LogError(ex, "Failed to publish login event to Kafka");
            throw;
        }
    }
}
