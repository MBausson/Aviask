using AviaskApi.Entities;

namespace AviaskApi.Services.Mailer;

public interface IMailerService
{
    public Task SendResetPasswordEmailAsync(AviaskUser user, string token);
    public Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
}