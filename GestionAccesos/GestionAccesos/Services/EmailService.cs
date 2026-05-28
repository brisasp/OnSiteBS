using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace GestionAccesos.Services;

public class EmailService
{
    private readonly ConfigEmail _configEmail;

    public EmailService(IOptions<ConfigEmail> configEmail)
    {
        _configEmail = configEmail.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body, MessagePriority priority)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(to))
            {
                throw new ArgumentNullException(nameof(to), "El correo del destinatario no puede ser nulo o vacío.");
            }

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(MailboxAddress.Parse(_configEmail.EmailFrom));
            emailMessage.To.Add(MailboxAddress.Parse(to));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                TextBody = body,
                HtmlBody = body
            };

            emailMessage.Body = bodyBuilder.ToMessageBody();

            using (var smtpClient = new SmtpClient())
            {
                // Conectamos al servidor SMTP
                await smtpClient.ConnectAsync(_configEmail.SmtpServer, _configEmail.SmtpPort,
                    SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(_configEmail.SmtpUsername, _configEmail.SmtpPassword);
                await smtpClient.SendAsync(emailMessage);
                await smtpClient.DisconnectAsync(true);
            }
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al enviar correo: {ex.Message}");
        }
    }

    public async Task SendEmailToAdminAsync(string subject, string htmlBody)
    {
        if (!string.IsNullOrWhiteSpace(_configEmail.AdminEmail))
            await SendEmailAsync(_configEmail.AdminEmail, subject, htmlBody, MessagePriority.Normal);
    }

    public async Task SendEmailWithAttachmentAsync(string to, string subject, string body, byte[] attachment,
        string attachmentFilename)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentNullException(nameof(to), "El correo del destinatario no puede ser nulo o vacío.");

            if (attachment == null || attachment.Length == 0)
                throw new ArgumentNullException(nameof(attachment), "El archivo adjunto no puede ser nulo o vacío.");

            if (string.IsNullOrWhiteSpace(attachmentFilename))
                throw new ArgumentNullException(nameof(attachmentFilename), "El nombre del archivo adjunto no puede ser nulo o vacío.");

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(MailboxAddress.Parse(_configEmail.EmailFrom));
            emailMessage.To.Add(MailboxAddress.Parse(to));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            bodyBuilder.Attachments.Add(attachmentFilename, new MemoryStream(attachment));
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using var smtpClient = new SmtpClient();
            await smtpClient.ConnectAsync(_configEmail.SmtpServer, _configEmail.SmtpPort, SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync(_configEmail.SmtpUsername, _configEmail.SmtpPassword);
            await smtpClient.SendAsync(emailMessage);
            await smtpClient.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al enviar correo con adjunto: {ex.Message}");
        }
    }
}