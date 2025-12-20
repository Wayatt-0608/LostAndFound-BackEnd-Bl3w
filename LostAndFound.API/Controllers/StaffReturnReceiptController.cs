using LostAndFound.API.DTOs;
using LostAndFound.Application.DTOs.ReturnReceipts;
using LostAndFound.Application.Interfaces;
using LostAndFound.Application.Interfaces.ReturnReceipts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LostAndFound.API.Controllers;

[ApiController]
[Route("api/staff-return-receipts")]
[Authorize(Roles = "Staff")]
public class StaffReturnReceiptController : ControllerBase
{
    private readonly IStaffReturnReceiptService _service;
    private readonly IImageUploadService _imageUploadService;

    public StaffReturnReceiptController(
        IStaffReturnReceiptService service,
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
    /// Tạo biên bản trả đồ (Staff) - Khi trả đồ thành công
    /// Tự động update: case.status = 'COMPLETED', found_item.status = 'RETURNED', claim.status = 'APPROVED'
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateStaffReturnReceiptFormRequest formRequest)
    {
        var staffId = GetCurrentUserId();
        
        try
        {
            // Upload receipt image nếu có
            string? receiptImageUrl = null;
            if (formRequest.ReceiptImage != null && formRequest.ReceiptImage.Length > 0)
            {
                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(formRequest.ReceiptImage.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { Message = "Định dạng file không hợp lệ. Chỉ chấp nhận: JPG, JPEG, PNG, GIF, WEBP" });
                }

                // Kiểm tra kích thước file (max 10MB)
                const long maxFileSize = 10 * 1024 * 1024; // 10MB
                if (formRequest.ReceiptImage.Length > maxFileSize)
                {
                    return BadRequest(new { Message = "Kích thước file quá lớn. Tối đa 10MB." });
                }

                // Upload ảnh lên Cloudinary
                using var stream = formRequest.ReceiptImage.OpenReadStream();
                receiptImageUrl = await _imageUploadService.UploadImageAsync(stream, formRequest.ReceiptImage.FileName);
                
                if (string.IsNullOrEmpty(receiptImageUrl))
                {
                    return BadRequest(new { Message = "Lỗi khi upload ảnh. Vui lòng thử lại." });
                }
            }

            // Tạo request object
            var request = new CreateStaffReturnReceiptRequest
            {
                CaseId = formRequest.CaseId,
                ClaimId = formRequest.ClaimId,
                ReceiptImageUrl = receiptImageUrl
            };

            var receipt = await _service.CreateAsync(staffId, request);
            return CreatedAtAction(nameof(GetById), new { id = receipt.Id }, receipt);
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
    /// Danh sách biên bản trả đồ
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var receipts = await _service.GetAllAsync();
        return Ok(receipts);
    }

    /// <summary>
    /// Chi tiết biên bản trả đồ
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var receipt = await _service.GetByIdAsync(id);
        if (receipt == null)
        {
            return NotFound(new { Message = "Không tìm thấy biên bản trả đồ." });
        }
        return Ok(receipt);
    }
}

