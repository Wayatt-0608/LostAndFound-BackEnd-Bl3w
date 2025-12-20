using LostAndFound.Application.DTOs;
using LostAndFound.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LostAndFound.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Gửi mã OTP đến email để đăng ký tài khoản
    /// </summary>
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Email không được để trống."
            });
        }

        var result = await _authService.SendOtpAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Đăng ký tài khoản mới với mã OTP
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.OtpCode))
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Vui lòng điền đầy đủ thông tin."
            });
        }

        if (request.Password.Length < 6)
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Mật khẩu phải có ít nhất 6 ký tự."
            });
        }

        // Validate role nếu có (nếu null sẽ được set mặc định là "Student" trong service)
        if (!string.IsNullOrWhiteSpace(request.Role) &&
            request.Role != "Student" && request.Role != "Staff" && request.Role != "SecurityOfficer")
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Role không hợp lệ. Chỉ chấp nhận: Student, Staff, SecurityOfficer"
            });
        }

        var result = await _authService.RegisterAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Đăng nhập tài khoản
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "Email và mật khẩu không được để trống."
            });
        }

        var result = await _authService.LoginAsync(request);
        
        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Quên mật khẩu - Gửi mã OTP đến email
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Email không được để trống."
            });
        }

        var result = await _authService.ForgotPasswordAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Đặt lại mật khẩu - Xác thực OTP và đổi password mới
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.OtpCode) ||
            string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Vui lòng điền đầy đủ thông tin."
            });
        }

        if (request.NewPassword.Length < 6)
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Mật khẩu mới phải có ít nhất 6 ký tự."
            });
        }

        var result = await _authService.ResetPasswordAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

