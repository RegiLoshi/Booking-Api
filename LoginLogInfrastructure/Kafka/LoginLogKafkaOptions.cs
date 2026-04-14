namespace LoginLogInfrastructure.Kafka;

public class LoginLogKafkaOptions
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string Topic { get; set; } = "user.login.succeeded";
    public string ConsumerGroupId { get; set; } = "login-log-consumer";
}
