namespace LostAndFound.Application.Interfaces;

public interface IEmailService
{
    Task SendOtpEmailAsync(string email, string otpCode);
    Task SendResetPasswordOtpEmailAsync(string email, string otpCode);
}

