using LostAndFound.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace LostAndFound.Application.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendOtpEmailAsync(string email, string otpCode)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        var smtpHost = emailSettings["SmtpHost"];
        var smtpPort = int.Parse(emailSettings["SmtpPort"]!);
        var smtpUser = emailSettings["SmtpUser"];
        var smtpPassword = emailSettings["SmtpPassword"];
        var fromEmail = emailSettings["FromEmail"];
        var fromName = emailSettings["FromName"];

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(smtpUser, smtpPassword)
        };

        var message = new MailMessage
        {
            From = new MailAddress(fromEmail!, fromName),
            Subject = "Mã OTP đăng ký tài khoản - Lost and Found System",
            Body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Mã OTP đăng ký tài khoản</h2>
                    <p>Xin chào,</p>
                    <p>Mã OTP của bạn là: <strong style='font-size: 24px; color: #007bff;'>{otpCode}</strong></p>
                    <p>Mã này có hiệu lực trong 10 phút.</p>
                    <p>Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>
                    <hr>
                    <p style='color: #666; font-size: 12px;'>Đây là email tự động, vui lòng không trả lời.</p>
                </body>
                </html>",
            IsBodyHtml = true
        };

        message.To.Add(email);

        await client.SendMailAsync(message);
    }

    public async Task SendResetPasswordOtpEmailAsync(string email, string otpCode)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        var smtpHost = emailSettings["SmtpHost"];
        var smtpPort = int.Parse(emailSettings["SmtpPort"]!);
        var smtpUser = emailSettings["SmtpUser"];
        var smtpPassword = emailSettings["SmtpPassword"];
        var fromEmail = emailSettings["FromEmail"];
        var fromName = emailSettings["FromName"];

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(smtpUser, smtpPassword)
        };

        var message = new MailMessage
        {
            From = new MailAddress(fromEmail!, fromName),
            Subject = "Mã OTP đặt lại mật khẩu - Lost and Found System",
            Body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Mã OTP đặt lại mật khẩu</h2>
                    <p>Xin chào,</p>
                    <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                    <p>Mã OTP của bạn là: <strong style='font-size: 24px; color: #007bff;'>{otpCode}</strong></p>
                    <p>Mã này có hiệu lực trong 10 phút.</p>
                    <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                    <p>Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>
                    <hr>
                    <p style='color: #666; font-size: 12px;'>Đây là email tự động, vui lòng không trả lời.</p>
                </body>
                </html>",
            IsBodyHtml = true
        };

        message.To.Add(email);

        await client.SendMailAsync(message);
    }
}

