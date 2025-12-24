using LostAndFound.API.DTOs;
using LostAndFound.Application.DTOs.LostReports;
using LostAndFound.Application.Interfaces;
using LostAndFound.Application.Interfaces.LostReports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LostAndFound.API.Controllers;

[ApiController]
[Route("api/student-lost-reports")]
[Authorize]
public class StudentLostReportController : ControllerBase
{
    private readonly IStudentLostReportService _service;
    private readonly IImageUploadService _imageUploadService;

    public StudentLostReportController(
        IStudentLostReportService service,
        IImageUploadService imageUploadService)
    {
        _service = service;
        _imageUploadService = imageUploadService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    /// <summary>
    /// Tạo báo mất (chỉ Student) - Nhận file ảnh trực tiếp
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Create([FromForm] CreateStudentLostReportFormRequest formRequest)
    {
        var studentId = GetCurrentUserId();

        try
        {
            // Validate file nếu có
            string? imageUrl = null;
            if (formRequest.Image != null && formRequest.Image.Length > 0)
            {
                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(formRequest.Image.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { Message = "Định dạng file không hợp lệ. Chỉ chấp nhận: JPG, JPEG, PNG, GIF, WEBP" });
                }

                // Kiểm tra kích thước file (max 10MB)
                const long maxFileSize = 10 * 1024 * 1024; // 10MB
                if (formRequest.Image.Length > maxFileSize)
                {
                    return BadRequest(new { Message = "Kích thước file quá lớn. Tối đa 10MB." });
                }

                // Upload ảnh lên Cloudinary
                using var stream = formRequest.Image.OpenReadStream();
                imageUrl = await _imageUploadService.UploadImageAsync(stream, formRequest.Image.FileName);

                if (string.IsNullOrEmpty(imageUrl))
                {
                    return BadRequest(new { Message = "Lỗi khi upload ảnh. Vui lòng thử lại." });
                }
            }

            // Tạo request object
            var request = new CreateStudentLostReportRequest
            {
                CategoryId = formRequest.CategoryId,
                Description = formRequest.Description,
                LostDate = formRequest.LostDate,
                LostLocation = formRequest.LostLocation,
                ImageUrl = imageUrl,
                IdentifyingFeatures = formRequest.IdentifyingFeatures,
                ClaimPassword = formRequest.ClaimPassword
            };

            var report = await _service.CreateAsync(studentId, request);
            return CreatedAtAction(nameof(GetById), new { id = report.Id }, report);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Xem báo mất của mình (chỉ Student)
    /// </summary>
    [HttpGet("my-reports")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyReports()
    {
        var studentId = GetCurrentUserId();
        var reports = await _service.GetMyReportsAsync(studentId);
        return Ok(reports);
    }

    /// <summary>
    /// Danh sách tất cả báo mất (chỉ Staff) - Xem toàn bộ lost reports của tất cả students
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> GetAll([FromQuery] int? categoryId)
    {
        // Staff mặc định được xem sensitive data
        var reports = await _service.GetAllAsync(categoryId, includeSensitiveData: true);
        return Ok(reports);
    }

    /// <summary>
    /// Chi tiết báo mất (Student xem của mình, Staff xem tất cả)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetCurrentUserId();
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // Student chỉ xem được của mình, Staff có thể xem tất cả
        int? studentId = userRole == "Student" ? userId : null;
        
        // Chỉ Staff/Security được xem sensitive data
        bool includeSensitiveData = userRole == "Staff" || userRole == "SecurityOfficer";

        var report = await _service.GetByIdAsync(id, studentId, includeSensitiveData);
        if (report == null)
        {
            return NotFound(new { Message = "Không tìm thấy báo mất." });
        }

        return Ok(report);
    }

    /// <summary>
    /// Cập nhật báo mất (chỉ Student, chỉ của mình, chưa có claim) - Nhận file ảnh trực tiếp
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateStudentLostReportFormRequest formRequest)
    {
        var studentId = GetCurrentUserId();

        try
        {
            // Upload ảnh nếu có
            string? imageUrl = null;
            if (formRequest.Image != null && formRequest.Image.Length > 0)
            {
                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(formRequest.Image.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { Message = "Định dạng file không hợp lệ. Chỉ chấp nhận: JPG, JPEG, PNG, GIF, WEBP" });
                }

                // Kiểm tra kích thước file (max 10MB)
                const long maxFileSize = 10 * 1024 * 1024; // 10MB
                if (formRequest.Image.Length > maxFileSize)
                {
                    return BadRequest(new { Message = "Kích thước file quá lớn. Tối đa 10MB." });
                }

                // Upload ảnh lên Cloudinary
                using var stream = formRequest.Image.OpenReadStream();
                imageUrl = await _imageUploadService.UploadImageAsync(stream, formRequest.Image.FileName);

                if (string.IsNullOrEmpty(imageUrl))
                {
                    return BadRequest(new { Message = "Lỗi khi upload ảnh. Vui lòng thử lại." });
                }
            }

            // Tạo request object
            var request = new UpdateStudentLostReportRequest
            {
                CategoryId = formRequest.CategoryId,
                Description = formRequest.Description,
                LostDate = formRequest.LostDate,
                LostLocation = formRequest.LostLocation,
                ImageUrl = imageUrl, // Nếu imageUrl là null, service sẽ giữ nguyên ảnh cũ
                IdentifyingFeatures = formRequest.IdentifyingFeatures,
                ClaimPassword = formRequest.ClaimPassword
            };

            var report = await _service.UpdateAsync(id, studentId, request);
            if (report == null)
            {
                return NotFound(new { Message = "Không tìm thấy báo mất hoặc không có quyền cập nhật." });
            }
            return Ok(report);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Xóa báo mất (chỉ Student, chỉ của mình, chưa có claim)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Delete(int id)
    {
        var studentId = GetCurrentUserId();

        try
        {
            var result = await _service.DeleteAsync(id, studentId);
            if (!result)
            {
                return NotFound(new { Message = "Không tìm thấy báo mất hoặc không có quyền xóa." });
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}

