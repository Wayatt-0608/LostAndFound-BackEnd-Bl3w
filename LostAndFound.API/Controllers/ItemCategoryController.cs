using LostAndFound.Application.DTOs.MasterData;
using LostAndFound.Application.Interfaces.MasterData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LostAndFound.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemCategoryController : ControllerBase
{
    private readonly IItemCategoryService _itemCategoryService;

    public ItemCategoryController(IItemCategoryService itemCategoryService)
    {
        _itemCategoryService = itemCategoryService;
    }

    /// <summary>
    /// Lấy danh sách tất cả item categories 
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Student,Staff,SecurityOfficer")]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _itemCategoryService.GetAllAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Lấy item category theo ID (chỉ Staff)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Student,Staff,SecurityOfficer")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _itemCategoryService.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound(new { Message = "Không tìm thấy item category." });
        }
        return Ok(category);
    }

    /// <summary>
    /// Tạo item category mới (chỉ Staff)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Create([FromBody] CreateItemCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { Message = "Tên category không được để trống." });
        }

        var category = await _itemCategoryService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    /// <summary>
    /// Cập nhật item category (chỉ Staff)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateItemCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { Message = "Tên category không được để trống." });
        }

        var category = await _itemCategoryService.UpdateAsync(id, request);
        if (category == null)
        {
            return NotFound(new { Message = "Không tìm thấy item category." });
        }
        return Ok(category);
    }

    /// <summary>
    /// Xóa item category (chỉ Staff)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _itemCategoryService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new { Message = "Không tìm thấy item category." });
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (DbUpdateException)
        {
            // Xử lý lỗi constraint từ database (fallback nếu kiểm tra ở service không bắt được)
            return BadRequest(new { Message = "Không thể xóa danh mục này vì nó đang được sử dụng trong hệ thống." });
        }
        catch (Exception)
        {
            return BadRequest(new { Message = "Đã xảy ra lỗi khi xóa item category." });
        }
    }
}

