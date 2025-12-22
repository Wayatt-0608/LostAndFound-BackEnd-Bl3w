using LostAndFound.API.DTOs;
using LostAndFound.Application.DTOs.SecurityVerification;
using LostAndFound.Application.Interfaces;
using LostAndFound.Application.Interfaces.SecurityVerification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LostAndFound.API.Controllers;

[ApiController]
[Route("api/security-verification-requests")]
[Authorize]
public class SecurityVerificationController : ControllerBase
{
    private readonly ISecurityVerificationService _service;
    private readonly IImageUploadService _imageUploadService;

    public SecurityVerificationController(
        ISecurityVerificationService service,
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
    /// Yêu cầu xác minh (Staff tạo) - Khi nhiều người claim hoặc cần xác minh
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> CreateRequest([FromBody] CreateSecurityVerificationRequestRequest request)
    {
        var requestedBy = GetCurrentUserId();
        
        try
        {
            var verificationRequest = await _service.CreateRequestAsync(requestedBy, request);
            return CreatedAtAction(nameof(GetRequestById), new { id = verificationRequest.Id }, verificationRequest);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Danh sách requests chờ xử lý (Security Officer)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SecurityOfficer")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var requests = await _service.GetPendingRequestsAsync();
        return Ok(requests);
    }

    /// <summary>
    /// Chi tiết request và decisions
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Staff,SecurityOfficer")]
    public async Task<IActionResult> GetRequestById(int id)
    {
        var request = await _service.GetRequestByIdAsync(id);
        if (request == null)
        {
            return NotFound(new { Message = "Không tìm thấy verification request." });
        }
        return Ok(request);
    }

    /// <summary>
    /// Security Officer đưa quyết định (APPROVED/REJECTED) - Nhận file ảnh trực tiếp
    /// </summary>
    [HttpPost("{id}/decisions")]
    [Authorize(Roles = "SecurityOfficer")]
    public async Task<IActionResult> CreateDecision(int id, [FromForm] CreateSecurityVerificationDecisionFormRequest formRequest)
    {
        var securityOfficerId = GetCurrentUserId();
        
        try
        {
            // Validate decision
            if (formRequest.Decision != "APPROVED" && formRequest.Decision != "REJECTED")
            {
                return BadRequest(new { Message = "Decision chỉ có thể là APPROVED hoặc REJECTED." });
            }

            // Upload evidence image nếu có
            string? evidenceImageUrl = null;
            if (formRequest.EvidenceImage != null && formRequest.EvidenceImage.Length > 0)
            {
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
                evidenceImageUrl = await _imageUploadService.UploadImageAsync(stream, formRequest.EvidenceImage.FileName);
                
                if (string.IsNullOrEmpty(evidenceImageUrl))
                {
                    return BadRequest(new { Message = "Lỗi khi upload ảnh. Vui lòng thử lại." });
                }
            }

            // Tạo decision request
            var decisionRequest = new CreateSecurityVerificationDecisionRequest
            {
                ClaimId = formRequest.ClaimId,
                Decision = formRequest.Decision,
                Note = formRequest.Note,
                EvidenceImageUrl = evidenceImageUrl
            };

            var result = await _service.CreateDecisionAsync(id, securityOfficerId, decisionRequest);
            return Ok(result);
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
}

