using Application.Abstractions.Email;
using Application.Configuration.Email;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Infrastructure.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;

    public SmtpEmailSender(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(_settings.SenderEmail)
            || string.IsNullOrWhiteSpace(_settings.Username)
            || string.IsNullOrWhiteSpace(_settings.Password))
        {
            throw new InvalidOperationException(
                "Configuração de e-mail incompleta (Email:SenderEmail/Username/Password). " +
                "Configure via 'dotnet user-secrets set' antes de enviar e-mails.");
        }

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        message.To.Add(to);

        await client.SendMailAsync(message);
    }
}
