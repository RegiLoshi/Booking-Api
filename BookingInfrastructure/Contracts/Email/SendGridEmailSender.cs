using BookingApplication.Abstractions.Contracts.Email;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BookingInfrastructure.Contracts.Email;

public class SendGridEmailSender(IOptions<SendGridOptions> options) : IEmailSender
{
    private readonly SendGridOptions _options = options.Value;

    public async Task SendAsync(
        string toEmail,
        string subject,
        string plainTextContent,
        string? htmlContent = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey) || string.IsNullOrWhiteSpace(_options.FromEmail))
            return;

        var client = new SendGridClient(_options.ApiKey);

        var from = new EmailAddress(_options.FromEmail, _options.FromName);
        var to = new EmailAddress(toEmail);

        var msg = MailHelper.CreateSingleEmail(
            from,
            to,
            subject,
            plainTextContent,
            htmlContent ?? plainTextContent);

        var response = await client.SendEmailAsync(msg, cancellationToken);
        if ((int)response.StatusCode >= 400)
        {
            var body = await response.Body.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"SendGrid send failed: {(int)response.StatusCode} {response.StatusCode}. {body}");
        }
    }
}

