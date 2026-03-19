using EmailService.Configuration;
using EmailService.Interfaces;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace EmailService.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;

        public SmtpEmailSender(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendAsync(string to, string subject, string html)
        {
            if (string.IsNullOrWhiteSpace(_settings.SmtpHost))
                throw new InvalidOperationException("EmailSettings:SmtpHost is required.");

            if (string.IsNullOrWhiteSpace(_settings.UserName))
                throw new InvalidOperationException("EmailSettings:UserName is required.");

            if (string.IsNullOrWhiteSpace(_settings.<secret>))
                throw new InvalidOperationException("EmailSettings:<secret> is required.");

            var fromAddress = new MailAddress(
                string.IsNullOrWhiteSpace(_settings.From) ? _settings.UserName : _settings.From,
                string.IsNullOrWhiteSpace(_settings.FromName) ? _settings.From : _settings.FromName);

            using var message = new MailMessage
            {
                From = fromAddress,
                Subject = subject,
                Body = html,
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(to));

            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = _settings.EnableSsl,
                Credentials = new NetworkCredential(_settings.UserName, _settings.<secret>)
            };

            await client.SendMailAsync(message);
        }
    }
}
