using LostAndFound.Application.DTOs.Cases;
using LostAndFound.Application.Interfaces.Cases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LostAndFound.API.Controllers;

[ApiController]
[Route("api/cases")]
[Authorize(Roles = "Staff,SecurityOfficer")]
public class CaseController : ControllerBase
{
    private readonly ICaseService _service;

    public CaseController(ICaseService service)
    {
        _service = service;
    }

    /// <summary>
    /// Danh sách cases (filter: campus, status)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? campusId,
        [FromQuery] string? status)
    {
        var cases = await _service.GetAllAsync(campusId, status);
        return Ok(cases);
    }

    /// <summary>
    /// Chi tiết case (bao gồm claims, verification requests)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var caseEntity = await _service.GetByIdAsync(id);
        if (caseEntity == null)
        {
            return NotFound(new { Message = "Không tìm thấy case." });
        }
        return Ok(caseEntity);
    }

    /// <summary>
    /// Cập nhật status case (OPEN, IN_PROGRESS, COMPLETED, FAILED)
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateCaseStatusRequest request)
    {
        try
        {
            var caseEntity = await _service.UpdateStatusAsync(id, request.Status);
            if (caseEntity == null)
            {
                return NotFound(new { Message = "Không tìm thấy case." });
            }
            return Ok(caseEntity);
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

