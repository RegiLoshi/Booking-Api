namespace BookingInfrastructure.Logging;

public class BookingKafkaOptions
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string Topic { get; set; } = "user.login.succeeded";
    public bool ProducerEnabled { get; set; }
}
