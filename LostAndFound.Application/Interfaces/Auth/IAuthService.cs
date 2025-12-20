using LostAndFound.Application.DTOs;

namespace LostAndFound.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> SendOtpAsync(SendOtpRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request);
}

