using System.Net;
using System.Net.Mail;
using AviaskApi.Configuration;
using AviaskApi.Entities;

namespace AviaskApi.Services.Mailer;

public class MailerService : IMailerService
{
    private static NetworkCredential? _credential;
    private readonly ILogger<MailerService> _logger;

    public MailerService(ILogger<MailerService> logger)
    {
        _logger = logger;

        _credential ??= new NetworkCredential(Env.Get("MAIL_EMAIL"), Env.Get("MAIL_PASSWORD"));
    }

    public virtual async Task SendResetPasswordEmailAsync(AviaskUser user, string token)
    {
        var url = $"{Env.Get("WEB_URL")}/users/recovery/{token}";

        var body = $"Dear <b>{user.UserName}</b>,<br /><br />" +
                   "We have received a request to reset the password associated with your account. " +
                   "To complete the password reset process, please navigate to the following address:<br />" +
                   $"{url}<br /><br />" +
                   "If you did not request this password reset, please ignore this email. " +
                   "Your account security is important to us, and no changes will be made.<br /><br />" +
                   "Best regards,<br />" +
                   "<b>The Aviask support</b>";

        await SendEmailAsync(user.Email!, "Password reset link", body, true);
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        var mail = new MailMessage(_credential!.UserName, to)
        {
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml
        };

        using var client = new SmtpClient("smtp.gmail.com", 587);

        client.EnableSsl = true;
        client.UseDefaultCredentials = false;
        client.Credentials = _credential;

        await client.SendMailAsync(mail);
        _logger.LogInformation($"Sent email to '{to}' : '{subject}'");
    }
}