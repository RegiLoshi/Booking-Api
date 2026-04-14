using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoginLogInfrastructure.Kafka;

public class LoginLogConsumerBackgroundService(
    IOptions<LoginLogKafkaOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<LoginLogConsumerBackgroundService> logger) : BackgroundService
{
    private readonly LoginLogKafkaOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(_options.Topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                if (result?.Message?.Value == null)
                    continue;

                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<LoginLogMessageProcessor>();
                await processor.ProcessAsync(result.Message.Value, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException ex)
            {
                logger.LogError(ex, "Kafka consume error in login log backend");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected login log consumer failure");
            }
        }

        consumer.Close();
    }
}
