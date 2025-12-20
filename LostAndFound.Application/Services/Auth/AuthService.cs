using LostAndFound.Application.DTOs;
using LostAndFound.Application.Interfaces;
using LostAndFound.Domain.Entities;
using LostAndFound.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LostAndFound.Application.Services;

public class AuthService : IAuthService
{
    private readonly IEmailService _emailService;
    private readonly IJwtService _jwtService;
    private readonly AppDbContext _context;
    private const string OTP_PURPOSE_REGISTER = "REGISTER";
    private const string OTP_PURPOSE_RESET_PASSWORD = "RESET_PASSWORD";

    public AuthService(
        IEmailService emailService,
        IJwtService jwtService,
        AppDbContext context)
    {
        _emailService = emailService;
        _jwtService = jwtService;
        _context = context;
    }

    public async Task<AuthResponse> SendOtpAsync(SendOtpRequest request)
    {
        // Kiểm tra email đã tồn tại chưa
        var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (emailExists)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Email đã được sử dụng. Vui lòng sử dụng email khác."
            };
        }

        // Tạo mã OTP 6 chữ số
        var random = new Random();
        var otpCode = random.Next(100000, 999999).ToString();

        // Lưu OTP vào database (expires sau 10 phút)
        var emailOtp = new EmailOtp
        {
            Email = request.Email,
            OtpCode = otpCode,
            Purpose = OTP_PURPOSE_REGISTER,
            CreatedAt = DateTime.Now,
            ExpiresAt = DateTime.Now.AddMinutes(10),
            IsUsed = false
        };

        _context.EmailOtps.Add(emailOtp);
        await _context.SaveChangesAsync();

        // Gửi email OTP
        try
        {
            await _emailService.SendOtpEmailAsync(request.Email, otpCode);
            return new AuthResponse
            {
                Success = true,
                Message = "Mã OTP đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư."
            };
        }
        catch (Exception ex)
        {
            return new AuthResponse
            {
                Success = false,
                Message = $"Không thể gửi email. Lỗi: {ex.Message}"
            };
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Kiểm tra email đã tồn tại chưa
        var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (emailExists)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Email đã được sử dụng."
            };
        }

        // Kiểm tra OTP
        var now = DateTime.Now;
        var validOtp = await _context.EmailOtps
            .Where(o => o.Email == request.Email 
                && o.Purpose == OTP_PURPOSE_REGISTER 
                && o.IsUsed == false 
                && o.ExpiresAt > now)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (validOtp == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Mã OTP không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu mã OTP mới."
            };
        }

        if (validOtp.OtpCode != request.OtpCode)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Mã OTP không đúng. Vui lòng kiểm tra lại."
            };
        }

        // Đánh dấu OTP đã sử dụng
        validOtp.IsUsed = true;
        await _context.SaveChangesAsync();

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, BCrypt.Net.BCrypt.GenerateSalt());

        // Set role mặc định là "Student" nếu null
        string role = string.IsNullOrWhiteSpace(request.Role) ? "Student" : request.Role;

        // Validate role
        if (role != "Student" && role != "Staff" && role != "SecurityOfficer")
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Role không hợp lệ. Chỉ chấp nhận: Student, Staff, SecurityOfficer"
            };
        }

        // Tạo user mới
        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            FullName = request.FullName,
            Role = role,
            StudentCode = request.StudentCode
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            Success = true,
            Message = "Đăng ký tài khoản thành công!"
        };
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // Tìm user theo email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Email hoặc mật khẩu không đúng."
            };
        }

        // Kiểm tra password
        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Email hoặc mật khẩu không đúng."
            };
        }

        // Tạo JWT token
        var token = _jwtService.GenerateToken(user);

        return new LoginResponse
        {
            Success = true,
            Message = "Đăng nhập thành công!",
            Token = token,
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                StudentCode = user.StudentCode,
                Role = user.Role,
                AvatarUrl = user.AvatarUrl
            }
        };
    }

    public async Task<AuthResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        // Kiểm tra email có tồn tại không
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            // Không báo lỗi chi tiết để tránh email enumeration attack
            return new AuthResponse
            {
                Success = true,
                Message = "Nếu email tồn tại, mã OTP đã được gửi đến email của bạn."
            };
        }

        // Tạo mã OTP 6 chữ số
        var random = new Random();
        var otpCode = random.Next(100000, 999999).ToString();

        // Lưu OTP vào database (expires sau 10 phút)
        var emailOtp = new EmailOtp
        {
            Email = request.Email,
            OtpCode = otpCode,
            Purpose = OTP_PURPOSE_RESET_PASSWORD,
            CreatedAt = DateTime.Now,
            ExpiresAt = DateTime.Now.AddMinutes(10),
            IsUsed = false
        };

        _context.EmailOtps.Add(emailOtp);
        await _context.SaveChangesAsync();

        // Gửi email OTP
        try
        {
            await _emailService.SendResetPasswordOtpEmailAsync(request.Email, otpCode);
            return new AuthResponse
            {
                Success = true,
                Message = "Nếu email tồn tại, mã OTP đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư."
            };
        }
        catch (Exception ex)
        {
            return new AuthResponse
            {
                Success = false,
                Message = $"Không thể gửi email. Lỗi: {ex.Message}"
            };
        }
    }

    public async Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        // Kiểm tra email có tồn tại không
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Email không tồn tại trong hệ thống."
            };
        }

        // Kiểm tra OTP
        var now = DateTime.Now;
        var validOtp = await _context.EmailOtps
            .Where(o => o.Email == request.Email 
                && o.Purpose == OTP_PURPOSE_RESET_PASSWORD 
                && o.IsUsed == false 
                && o.ExpiresAt > now)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (validOtp == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Mã OTP không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu mã OTP mới."
            };
        }

        if (validOtp.OtpCode != request.OtpCode)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Mã OTP không đúng. Vui lòng kiểm tra lại."
            };
        }

        // Đánh dấu OTP đã sử dụng
        validOtp.IsUsed = true;

        // Hash password mới
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, BCrypt.Net.BCrypt.GenerateSalt());

        // Cập nhật password
        user.PasswordHash = passwordHash;
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            Success = true,
            Message = "Đổi mật khẩu thành công! Bạn có thể đăng nhập với mật khẩu mới."
        };
    }
}

