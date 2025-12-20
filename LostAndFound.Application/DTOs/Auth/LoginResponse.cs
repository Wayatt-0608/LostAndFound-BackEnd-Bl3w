namespace LostAndFound.Application.DTOs;

public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public string? Token { get; set; }
    public UserInfo? User { get; set; }
}

public class UserInfo
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string? FullName { get; set; }
    public string? StudentCode { get; set; }
    public string Role { get; set; } = null!;
    public string? AvatarUrl { get; set; }
}

