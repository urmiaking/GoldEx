using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Shared.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace GoldEx.Server.Infrastructure.Services;

[TransientService]
public class EmailSender(IOptions<EmailSettings> emailSettings, ILogger<EmailSender> logger) : IEmailSender<AppUser>
{
    private readonly EmailSettings _emailSettings = emailSettings.Value;

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlMessage };

            using var client = new SmtpClient();

            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
            await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            logger.LogError($"Error sending email: {ex.Message}");
            throw;
        }
    }

    public async Task SendConfirmationLinkAsync(AppUser user, string email, string confirmationLink)
    {
        var message = $"لطفا برای تکمیل ثبت نام و فعالسازی حساب <a href='{confirmationLink}'>اینجا</a> را کلیک کنید.";
        await SendEmailAsync(email, "فعالسازی حساب کاربری", message);
    }

    public async Task SendPasswordResetLinkAsync(AppUser user, string email, string resetLink)
    {
        var message = $"لطفا برای بازنشانی کلمه عبور <a href='{resetLink}'>اینجا</a> را کلیک کنید.";
        await SendEmailAsync(email, "بازنشانی کلمه عبور", message);
    }

    public async Task SendPasswordResetCodeAsync(AppUser user, string email, string resetCode)
    {
        // For sending a code (less common, but included for completeness)
        var message = $"کد بازنشانی رمز عبور شما: {resetCode}";
        await SendEmailAsync(email, "کد بازنشانی رمز عبور", message);
    }
}