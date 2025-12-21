using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LostAndFound.Application.DTOs.MasterData;
using LostAndFound.Application.Interfaces.MasterData;
using LostAndFound.Domain.Entities;
using LostAndFound.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LostAndFound.Application.Services.MasterData;

public class ItemCategoryService : IItemCategoryService
{
    private readonly AppDbContext _context;

    public ItemCategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ItemCategoryResponse>> GetAllAsync()
    {
        var categories = await _context.ItemCategories
            .OrderBy(c => c.Name)
            .ToListAsync();

        return categories.Select(c => new ItemCategoryResponse
        {
            Id = c.Id,
            Name = c.Name
        });
    }

    public async Task<ItemCategoryResponse?> GetByIdAsync(int id)
    {
        var category = await _context.ItemCategories.FindAsync(id);
        if (category == null)
            return null;

        return new ItemCategoryResponse
        {
            Id = category.Id,
            Name = category.Name
        };
    }

    public async Task<ItemCategoryResponse> CreateAsync(CreateItemCategoryRequest request)
    {
        var category = new ItemCategory
        {
            Name = request.Name
        };

        _context.ItemCategories.Add(category);
        await _context.SaveChangesAsync();

        return new ItemCategoryResponse
        {
            Id = category.Id,
            Name = category.Name
        };
    }

    public async Task<ItemCategoryResponse?> UpdateAsync(int id, UpdateItemCategoryRequest request)
    {
        var category = await _context.ItemCategories.FindAsync(id);
        if (category == null)
            return null;

        category.Name = request.Name;

        _context.ItemCategories.Update(category);
        await _context.SaveChangesAsync();

        return new ItemCategoryResponse
        {
            Id = category.Id,
            Name = category.Name
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.ItemCategories.FindAsync(id);
        if (category == null)
            return false;

        // Kiểm tra xem category có đang được sử dụng trong StudentLostReports không
        var isUsedInLostReports = await _context.StudentLostReports.AnyAsync(lr => lr.CategoryId == id);
        if (isUsedInLostReports)
        {
            throw new InvalidOperationException("Không thể xóa danh mục này vì nó đang được sử dụng trong các báo mất của sinh viên.");
        }

        // Kiểm tra xem category có đang được sử dụng trong StaffFoundItems không
        var isUsedInFoundItems = await _context.StaffFoundItems.AnyAsync(fi => fi.CategoryId == id);
        if (isUsedInFoundItems)
        {
            throw new InvalidOperationException("Không thể xóa danh mục này vì nó đang được sử dụng trong các đồ nhặt được.");
        }

        _context.ItemCategories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }
}


