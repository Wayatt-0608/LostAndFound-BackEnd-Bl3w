namespace LostAndFound.Application.DTOs;

public class RegisterRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string OtpCode { get; set; } = null!;
    public string? StudentCode { get; set; }
    public string? Role { get; set; } // "Student", "Staff", "SecurityOfficer" - mặc định là "Student" nếu null
}

