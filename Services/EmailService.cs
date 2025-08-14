using System.Net.Mail;
using System.Net;

namespace bot_social_media.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var smtpHost = _config["Email:Host"];
            var smtpPort = int.Parse(_config["Email:Port"] ?? "587");
            var fromEmail = _config["Email:FromEmail"];
            var apiKey = _config["Email:ApiKey"]; // gunakan API key, bukan password

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(fromEmail, apiKey),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
    }
}
