using LostAndFound.API.DTOs;
using LostAndFound.Application.DTOs.Claims;
using LostAndFound.Application.Interfaces;
using LostAndFound.Application.Interfaces.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LostAndFound.API.Controllers;

[ApiController]
[Route("api/student-claims")]
[Authorize]
public class StudentClaimController : ControllerBase
{
    private readonly IStudentClaimService _service;
    private readonly IImageUploadService _imageUploadService;

    public StudentClaimController(
        IStudentClaimService service,
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
    /// Tạo claim (có thể link với lost_report_id) - Chỉ Student
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Create([FromBody] CreateStudentClaimRequest request)
    {
        var studentId = GetCurrentUserId();
        
        try
        {
            var claim = await _service.CreateAsync(studentId, request);
            return CreatedAtAction(nameof(GetById), new { id = claim.Id }, claim);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
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
    /// Xem claims của mình - Chỉ Student
    /// </summary>
    [HttpGet("my-claims")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyClaims()
    {
        var studentId = GetCurrentUserId();
        var claims = await _service.GetMyClaimsAsync(studentId);
        return Ok(claims);
    }

    /// <summary>
    /// Danh sách tất cả claims (chỉ Staff/Security) - Xem toàn bộ claims của tất cả students
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Staff,SecurityOfficer")]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] int? caseId)
    {
        var claims = await _service.GetAllAsync(status, caseId);
        return Ok(claims);
    }

    /// <summary>
    /// Chi tiết claim - Student xem của mình, Staff/Security xem tất cả
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetCurrentUserId();
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // Student chỉ xem được của mình, Staff/Security có thể xem tất cả
        int? studentId = userRole == "Student" ? userId : null;

        var claim = await _service.GetByIdAsync(id, studentId);
        if (claim == null)
        {
            return NotFound(new { Message = "Không tìm thấy claim." });
        }
        return Ok(claim);
    }

    /// <summary>
    /// Upload evidence image - Chỉ Student, chỉ của mình, chỉ khi status = PENDING
    /// </summary>
    [HttpPut("{id}/evidence")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> UpdateEvidence(int id, [FromForm] UpdateStudentClaimEvidenceFormRequest formRequest)
    {
        var studentId = GetCurrentUserId();
        
        try
        {
            // Validate file
            if (formRequest.EvidenceImage == null || formRequest.EvidenceImage.Length == 0)
            {
                return BadRequest(new { Message = "Vui lòng chọn file ảnh evidence." });
            }

            // Kiểm tra định dạng file
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(formRequest.EvidenceImage.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { Message = "Định dạng file không hợp lệ. Chỉ chấp nhận: JPG, JPEG, PNG, GIF, WEBP" });
            }

            // Kiểm tra kích thước file (max 10MB)
            const long maxFileSize = 10 * 1024 * 1024; // 10MB
            if (formRequest.EvidenceImage.Length > maxFileSize)
            {
                return BadRequest(new { Message = "Kích thước file quá lớn. Tối đa 10MB." });
            }

            // Upload ảnh lên Cloudinary
            using var stream = formRequest.EvidenceImage.OpenReadStream();
            var evidenceImageUrl = await _imageUploadService.UploadImageAsync(stream, formRequest.EvidenceImage.FileName);
            
            if (string.IsNullOrEmpty(evidenceImageUrl))
            {
                return BadRequest(new { Message = "Lỗi khi upload ảnh. Vui lòng thử lại." });
            }

            var claim = await _service.UpdateEvidenceAsync(id, studentId, evidenceImageUrl);
            if (claim == null)
            {
                return NotFound(new { Message = "Không tìm thấy claim hoặc không có quyền cập nhật." });
            }
            return Ok(claim);
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
    /// Staff match lost report với found item và tạo claim tự động cho student
    /// </summary>
    [HttpPost("staff/match")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> StaffMatchLostReport([FromBody] CreateStaffMatchClaimRequest request)
    {
        try
        {
            var claim = await _service.CreateClaimForStudentAsync(request.LostReportId, request.FoundItemId);
            return Ok(claim);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { Message = ex.Message });
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
    /// Staff approve claim trực tiếp (chỉ khi case chỉ có 1 claim) - Chỉ Staff
    /// </summary>
    [HttpPut("{id}/staff-approve")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> StaffApproveClaim(int id)
    {
        try
        {
            var claim = await _service.ApproveClaimByStaffAsync(id);
            return Ok(claim);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { Message = ex.Message });
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

