namespace LostAndFound.Application.DTOs;

public class ResetPasswordRequest
{
    public string Email { get; set; } = null!;
    public string OtpCode { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}

