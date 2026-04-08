namespace BookingApplication.Abstractions.Contracts.Email;

public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string plainTextContent, string? htmlContent = null, CancellationToken cancellationToken = default);
}

