using LostAndFound.Application.DTOs.MasterData;
using LostAndFound.Application.Interfaces.MasterData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LostAndFound.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CampusController : ControllerBase
{
    private readonly ICampusService _campusService;

    public CampusController(ICampusService campusService)
    {
        _campusService = campusService;
    }

    /// <summary>
    /// Debug endpoint - kiểm tra token và claims (không cần auth)
    /// </summary>
    [HttpGet("test-token")]
    [AllowAnonymous]
    public IActionResult TestToken()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        var hasBearer = authHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ?? false;
        var tokenOnly = hasBearer ? authHeader?.Substring(7) : authHeader;

        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Ok(new
            {
                IsAuthenticated = false,
                Message = "Token chưa được authenticate hoặc không hợp lệ",
                AuthHeader = authHeader != null ? "Có Authorization header" : "Không có Authorization header",
                HasBearerPrefix = hasBearer,
                TokenLength = tokenOnly?.Length ?? 0,
                Suggestion = hasBearer ? "Token có Bearer prefix" : "Token cần có format: Bearer <token>"
            });
        }

        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        var roles = User.FindAll("role").Select(c => c.Value).ToList();
        var roleClaims = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var isInRole = User.IsInRole("Staff");

        return Ok(new
        {
            IsAuthenticated = true,
            Claims = claims,
            RoleClaims = roles,
            RoleClaimsLegacy = roleClaims,
            IsInRoleStaff = isInRole,
            IdentityName = User.Identity?.Name,
            AuthenticationType = User.Identity?.AuthenticationType,
            AuthHeader = authHeader != null ? "Có Authorization header" : "Không có Authorization header",
            HasBearerPrefix = hasBearer
        });
    }


    /// <summary>
    /// Lấy danh sách tất cả campus (chỉ Staff)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> GetAll()
    {
        var campuses = await _campusService.GetAllAsync();
        return Ok(campuses);
    }

    /// <summary>
    /// Lấy campus theo ID (chỉ Staff)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> GetById(int id)
    {
        var campus = await _campusService.GetByIdAsync(id);
        if (campus == null)
        {
            return NotFound(new { Message = "Không tìm thấy campus." });
        }
        return Ok(campus);
    }

    /// <summary>
    /// Tạo campus mới (chỉ Staff)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Create([FromBody] CreateCampusRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { Message = "Tên campus không được để trống." });
        }

        var campus = await _campusService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = campus.Id }, campus);
    }

    /// <summary>
    /// Cập nhật campus (chỉ Staff)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCampusRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { Message = "Tên campus không được để trống." });
        }

        var campus = await _campusService.UpdateAsync(id, request);
        if (campus == null)
        {
            return NotFound(new { Message = "Không tìm thấy campus." });
        }
        return Ok(campus);
    }

    /// <summary>
    /// Xóa campus (chỉ Staff)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _campusService.DeleteAsync(id);
        if (!result)
        {
            return NotFound(new { Message = "Không tìm thấy campus." });
        }
        return NoContent();
    }
}

