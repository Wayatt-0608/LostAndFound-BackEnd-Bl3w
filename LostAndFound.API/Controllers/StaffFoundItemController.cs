using LostAndFound.API.DTOs;
using LostAndFound.Application.DTOs.FoundItems;
using LostAndFound.Application.Interfaces;
using LostAndFound.Application.Interfaces.FoundItems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LostAndFound.API.Controllers;

[ApiController]
[Route("api/staff-found-items")]
[Authorize] // Cho phép tất cả authenticated users (Student, Staff, Security)
public class StaffFoundItemController : ControllerBase
{
    private readonly IStaffFoundItemService _service;
    private readonly IImageUploadService _imageUploadService;

    public StaffFoundItemController(
        IStaffFoundItemService service,
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
    /// Tạo đồ nhặt được (Staff/Security) - Nhận file ảnh trực tiếp
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Staff,SecurityOfficer")]
    public async Task<IActionResult> Create([FromForm] CreateStaffFoundItemFormRequest formRequest)
    {
        var createdBy = GetCurrentUserId();
        
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
            var request = new CreateStaffFoundItemRequest
            {
                CategoryId = formRequest.CategoryId,
                CampusId = formRequest.CampusId,
                Description = formRequest.Description,
                FoundDate = formRequest.FoundDate,
                ImageUrl = imageUrl
            };

            var foundItem = await _service.CreateAsync(createdBy, request);
            return CreatedAtAction(nameof(GetById), new { id = foundItem.Id }, foundItem);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Danh sách đồ nhặt được (filter: campus, status, category) - Staff/Security/Student đều xem được
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? campusId,
        [FromQuery] string? status,
        [FromQuery] int? categoryId)
    {
        // Chỉ hiển thị đồ nhặt được có status = STORED cho Student
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (userRole == "Student")
        {
            status = "STORED"; // Force chỉ hiển thị STORED cho Student
        }

        var items = await _service.GetAllAsync(campusId, status, categoryId);
        return Ok(items);
    }

    /// <summary>
    /// Chi tiết đồ nhặt được - Staff/Security/Student đều xem được
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var foundItem = await _service.GetByIdAsync(id);
        if (foundItem == null)
        {
            return NotFound(new { Message = "Không tìm thấy đồ nhặt được." });
        }
        return Ok(foundItem);
    }

    /// <summary>
    /// Cập nhật đồ nhặt được - Nhận file ảnh trực tiếp (Staff/Security)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Staff,SecurityOfficer")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateStaffFoundItemFormRequest formRequest)
    {
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
            var request = new UpdateStaffFoundItemRequest
            {
                CategoryId = formRequest.CategoryId,
                CampusId = formRequest.CampusId,
                Description = formRequest.Description,
                FoundDate = formRequest.FoundDate,
                ImageUrl = imageUrl // Nếu imageUrl là null, service sẽ giữ nguyên ảnh cũ
            };

            var foundItem = await _service.UpdateAsync(id, request);
            if (foundItem == null)
            {
                return NotFound(new { Message = "Không tìm thấy đồ nhặt được." });
            }
            return Ok(foundItem);
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
    /// Đổi status (STORED/RETURNED) - Staff/Security
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Staff,SecurityOfficer")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStaffFoundItemStatusRequest request)
    {
        try
        {
            var foundItem = await _service.UpdateStatusAsync(id, request.Status);
            if (foundItem == null)
            {
                return NotFound(new { Message = "Không tìm thấy đồ nhặt được." });
            }
            return Ok(foundItem);
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
}

