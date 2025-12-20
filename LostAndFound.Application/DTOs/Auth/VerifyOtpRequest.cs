namespace LostAndFound.Application.DTOs;

public class VerifyOtpRequest
{
    public string Email { get; set; } = null!;
    public string OtpCode { get; set; } = null!;
}

