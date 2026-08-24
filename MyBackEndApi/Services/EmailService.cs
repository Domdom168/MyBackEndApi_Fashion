using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;

namespace MyBackEndApi.Services
{
    public class EmailService : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // For development, log the email instead of sending
            if (_config["EmailSettings:UseConsoleFallback"] == "true")
            {
                _logger.LogInformation($"Email to {to}: {subject}");
                _logger.LogInformation($"Body: {body}");
                return;
            }

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    _config["EmailSettings:FromName"] ?? "Fashion Store",
                    _config["EmailSettings:FromEmail"] ?? "domkodomo2@gmail.com"
                ));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = body };

                using var client = new SmtpClient();

                // Connect
                await client.ConnectAsync(
                    _config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com",
                    int.TryParse(_config["EmailSettings:SmtpPort"], out int port) ? port : 587,
                    SecureSocketOptions.StartTls
                );

                // Authenticate
                var username = _config["EmailSettings:SmtpUsername"] ?? "";
                var password = _config["EmailSettings:SmtpPassword"] ?? "";
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    await client.AuthenticateAsync(username, password);
                }
                else
                {
                    _logger.LogWarning("SMTP credentials not configured. Email not sent.");
                    return;
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                _logger.LogInformation($"Email sent to {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {to}");
                // Re-throw or handle as needed
                throw;
            }
        }
    }
}
